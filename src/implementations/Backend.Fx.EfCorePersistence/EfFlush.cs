using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Exceptions;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Backend.Fx.EfCorePersistence
{
    public class EfFlush : ICanFlush
    {
        private static readonly ILogger Logger = LogManager.Create<EfFlush>();
        public DbContext DbContext { get; }
        public ICurrentTHolder<IIdentity> IdentityHolder { get; }
        public IClock Clock { get; }

        public EfFlush(DbContext dbContext, ICurrentTHolder<IIdentity> identityHolder, IClock clock)
        {
            DbContext = dbContext;
            Logger.Info("Disabling auto detect changes on this DbContext. Changes will be detected explicitly when flushing.");
            DbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            IdentityHolder = identityHolder;
            Clock = clock;
        }

        public void Flush()
        {
            DetectChanges();
            UpdateTrackingProperties();
            DbContext.TraceChangeTrackerState();
            CheckForMissingTenantIds();
            SaveChanges();
        }

        private void DetectChanges()
        {
            using (Logger.DebugDuration("Detecting changes"))
            {
                DbContext.ChangeTracker.DetectChanges();
            }
        }

        private void UpdateTrackingProperties()
        {
            using (Logger.DebugDuration("Updating tracking properties of created and modified entities"))
            {
                UpdateTrackingProperties(IdentityHolder.Current.Name, Clock.UtcNow);
            }
        }

        private void CheckForMissingTenantIds()
        {
            using (Logger.DebugDuration("Checking for missing tenant ids"))
            {
                AggregateRoot[] aggregatesWithoutTenantId = DbContext
                                                            .ChangeTracker
                                                            .Entries()
                                                            .Where(e => e.State == EntityState.Added)
                                                            .Select(e => e.Entity)
                                                            .OfType<AggregateRoot>()
                                                            .Where(ent => ent.TenantId == 0)
                                                            .ToArray();
                if (aggregatesWithoutTenantId.Length > 0)
                {
                    throw new InvalidOperationException(
                        $"Attempt to save aggregate root entities without tenant id: {string.Join(",", aggregatesWithoutTenantId.Select(agg => agg.DebuggerDisplay))}");
                }
            }
        }
        
        private void SaveChanges()
        {
            using (Logger.DebugDuration("Saving changes"))
            {
                try
                {
                    DbContext.SaveChanges();
                }
                catch (DbUpdateConcurrencyException concurrencyException)
                {
                    throw new ConflictedException("Saving changes failed due to optimistic concurrency violation", concurrencyException);
                }
            }
        }
        
        private void UpdateTrackingProperties(string userId, DateTime utcNow)
        {
            userId ??= "anonymous";
            var isTraceEnabled = Logger.IsTraceEnabled();
            var count = 0;

            // Modifying an entity (also removing an entity from an aggregate) should leave the aggregate root as modified
            DbContext.ChangeTracker
                     .Entries<Entity>()
                     .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                     .Where(entry => !(entry.Entity is AggregateRoot))
                     .ToArray()
                     .ForAll(entry =>
                     {
                         EntityEntry aggregateRootEntry = FindAggregateRootEntry(DbContext.ChangeTracker, entry);
                         if (aggregateRootEntry == null)
                         {
                             throw new InvalidOperationException($"Could not find aggregate root of {entry.Entity.GetType().Name}[{entry.Entity?.Id}]");
                         }
                         else if (aggregateRootEntry.State == EntityState.Unchanged)
                         {
                             aggregateRootEntry.State = EntityState.Modified;
                         }
                     });

            DbContext.ChangeTracker
                     .Entries<Entity>()
                     .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified)
                     .ForAll(entry =>
                     {
                         try
                         {
                             count++;
                             Entity entity = entry.Entity;

                             if (entry.State == EntityState.Added)
                             {
                                 if (isTraceEnabled) Logger.Trace("tracking that {0}[{1}] was created by {2} at {3:T} UTC", entity.GetType().Name, entity.Id, userId, utcNow);
                                 entity.SetCreatedProperties(userId, utcNow);
                             }
                             else if (entry.State == EntityState.Modified)
                             {
                                 if (isTraceEnabled) Logger.Trace("tracking that {0}[{1}] was modified by {2} at {3:T} UTC", entity.GetType().Name, entity.Id, userId, utcNow);
                                 entity.SetModifiedProperties(userId, utcNow);

                                 // this line causes the recent changes of tracking properties to be detected before flushing
                                 entry.State = EntityState.Modified;
                             }
                         }
                         catch (Exception ex)
                         {
                             Logger.Warn(ex, "Updating tracking properties failed");
                             throw;
                         }
                     });
            if (count > 0) Logger.Debug($"Tracked {count} entities as created/changed on {utcNow:u} by {userId}");
        }

        /// <summary>
        ///     This method finds the EntityEntry&lt;AggregateRoot&gt; of an EntityEntry&lt;Entity&gt;
        ///     assuming it has been loaded and is being tracked by the change tracker.
        /// </summary>
        private static EntityEntry FindAggregateRootEntry(ChangeTracker changeTracker, EntityEntry entry)
        {
            Logger.Debug($"Searching aggregate root of {entry.Entity.GetType().Name}[{(entry.Entity as Identified)?.Id}]");
            foreach (NavigationEntry navigation in entry.Navigations)
            {
                TypeInfo navTargetTypeInfo = navigation.Metadata.TargetEntityType.ClrType.GetTypeInfo();
                int navigationTargetForeignKeyValue;

                if (navigation.CurrentValue == null)
                {
                    var navigationMetadata = ((INavigation)navigation.Metadata);
                    // orphaned entity, original value contains the foreign key value
                    if (navigationMetadata.ForeignKey.Properties.Count > 1) throw new InvalidOperationException("Foreign Keys with multiple properties are not supported.");

                    IProperty property = navigationMetadata.ForeignKey.Properties[0];
                    navigationTargetForeignKeyValue = (int) entry.OriginalValues[property];
                }
                else
                {
                    // added or modified entity, current value contains the foreign key value
                    navigationTargetForeignKeyValue = ((Entity) navigation.CurrentValue).Id;
                }

                // assumption: an entity cannot be loaded on its own. Everything on the navigation path starting from the 
                // aggregate root must have been loaded before, therefore we can find it using the change tracker
                var navigationTargetEntry = changeTracker
                                            .Entries<Entity>()
                                            .Single(ent => Equals(ent.Entity.GetType().GetTypeInfo(), navTargetTypeInfo)
                                                           && ent.Property(nameof(Entity.Id)).CurrentValue.Equals(navigationTargetForeignKeyValue));

                // if the target is AggregateRoot, no (further) recursion is needed
                if (typeof(AggregateRoot).GetTypeInfo().IsAssignableFrom(navTargetTypeInfo)) return navigationTargetEntry;

                // recurse in case of "Entity -> Entity -> AggregateRoot"
                Logger.Debug("Recursing...");
                return FindAggregateRootEntry(changeTracker, navigationTargetEntry);
            }

            return null;
        }
    }
}