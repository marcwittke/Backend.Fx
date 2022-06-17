using System;
using System.Diagnostics.CodeAnalysis;
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
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.EfCore6Persistence
{
    public class EfFlush : ICanFlush
    {
        private static readonly ILogger Logger = Log.Create<EfFlush>();
        public DbContext DbContext { get; }
        public ICurrentTHolder<IIdentity> IdentityHolder { get; }
        public IClock Clock { get; }

        public EfFlush(DbContext dbContext, ICurrentTHolder<IIdentity> identityHolder, IClock clock)
        {
            DbContext = dbContext;
            DbContext.DisableChangeTracking();
            IdentityHolder = identityHolder;
            Clock = clock;
        }

        public void Flush()
        {
            DetectChanges();
            UpdateTrackingProperties();
            if (Logger.IsEnabled(LogLevel.Trace)) Logger.LogTrace("Change tracker state: {@ChangeTrackerState}", DbContext.ChangeTracker.DebugView.LongView);
            CheckForMissingTenantIds();
            SaveChanges();
        }

        private void DetectChanges()
        {
            using (Logger.LogDebugDuration("Detecting changes"))
            {
                DbContext.ChangeTracker.DetectChanges();
            }
        }

        private void UpdateTrackingProperties()
        {
            using (Logger.LogDebugDuration("Updating tracking properties of created and modified entities"))
            {
                UpdateTrackingProperties(IdentityHolder.Current.Name, Clock.UtcNow);
            }
        }

        private void CheckForMissingTenantIds()
        {
            using (Logger.LogDebugDuration("Checking for missing tenant ids"))
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
            using (Logger.LogDebugDuration("Saving changes"))
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
        
        private void UpdateTrackingProperties(string identity, DateTime utcNow)
        {
            identity ??= "anonymous";
            var isTraceEnabled = Logger.IsEnabled(LogLevel.Trace);
            var count = 0;

            // Modifying an entity (also removing an entity from an aggregate) should leave the aggregate root as modified
            DbContext.ChangeTracker
                     .Entries<Entity>()
                     .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                     .Where(entry => !(entry.Entity is AggregateRoot))
                     .ToArray()
                     .ForAll(entry =>
                     {
                         EntityEntry aggregateRootEntry = GetAggregateRootEntry(DbContext.ChangeTracker, entry);
                         if (aggregateRootEntry.State == EntityState.Unchanged) aggregateRootEntry.State = EntityState.Modified;
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
                                 if (isTraceEnabled) Logger.LogTrace("tracking that {EntityTypeName}[{Id}] was created by {Identity} at {UtcNow}", entity.GetType().Name, entity.Id, identity, utcNow);
                                 entity.SetCreatedProperties(identity, utcNow);
                             }
                             else if (entry.State == EntityState.Modified)
                             {
                                 if (isTraceEnabled) Logger.LogTrace("tracking that {EntityTypeName}[{Id}] was modified by {Identity} at {UtcNow}", entity.GetType().Name, entity.Id, identity, utcNow);
                                 entity.SetModifiedProperties(identity, utcNow);

                                 // this line causes the recent changes of tracking properties to be detected before flushing
                                 entry.State = EntityState.Modified;
                             }
                         }
                         catch (Exception ex)
                         {
                             Logger.LogWarning(ex, "Updating tracking properties failed");
                             throw;
                         }
                     });
            if (count > 0) Logger.LogDebug("Tracked {EntityCount} entities as created/changed on {UtcNow} by {Identity}", count, utcNow, identity);
        }
        
        /// <summary>
        ///     This method finds the EntityEntry&lt;AggregateRoot&gt; of an EntityEntry&lt;Entity&gt;
        ///     assuming it has been loaded and is being tracked by the change tracker.
        /// </summary>
        [return: NotNull]
        private static EntityEntry GetAggregateRootEntry(ChangeTracker changeTracker, EntityEntry entry)
        {
            Logger.LogDebug("Searching aggregate root of {EntityTypeName}[{Id}]", entry.Entity.GetType().Name, (entry.Entity as Identified)?.Id);
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
                Logger.LogDebug("Recursing...");
                return GetAggregateRootEntry(changeTracker, navigationTargetEntry);
            }
            
            throw new InvalidOperationException($"Could not find aggregate root of {entry.Entity.GetType().Name}[{(entry.Entity as Identified)?.Id}]");
        }
    }
}