namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using BuildingBlocks;
    using Extensions;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Metadata;

    public static class DbContextExtensions
    {
        private static readonly ILogger Logger = LogManager.Create(typeof(DbContextExtensions));

        public static void RegisterRowVersionProperty(this ModelBuilder modelBuilder)
        {
            modelBuilder.Model
                        .GetEntityTypes()
                        .Where(mt => typeof(Entity).GetTypeInfo().IsAssignableFrom(mt.ClrType.GetTypeInfo()))
                        .ForAll(mt => modelBuilder.Entity(mt.ClrType).Property<byte[]>("RowVersion").IsRowVersion());
        }

        public static void RegisterEntityIdAsNeverGenerated(this ModelBuilder modelBuilder)
        {
            modelBuilder.Model
                        .GetEntityTypes()
                        .Where(mt => typeof(Entity).GetTypeInfo().IsAssignableFrom(mt.ClrType.GetTypeInfo()))
                        .ForAll(mt => modelBuilder.Entity(mt.ClrType).Property(nameof(Entity.Id)).ValueGeneratedNever());
        }

        public static void ApplyAggregateRootMappings(this DbContext dbContext, ModelBuilder modelBuilder)
        {
            //CAVE: IAggregateRootMapping implementations must reside in the same assembly as the Applications DbContext-type
            var aggregateDefinitionTypeInfos = dbContext
                .GetType()
                .GetTypeInfo()
                .Assembly
                .ExportedTypes
                .Select(t => t.GetTypeInfo())
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(IAggregateRootMapping).GetTypeInfo().IsAssignableFrom(t));
            foreach (var typeInfo in aggregateDefinitionTypeInfos)
            {
                IAggregateRootMapping aggregateRootMapping = (IAggregateRootMapping)Activator.CreateInstance(typeInfo.AsType());
                aggregateRootMapping.ApplyEfMapping(modelBuilder);
            }
        }

        public static void UpdateTrackingProperties(this DbContext dbContext, string userId, DateTime utcNow)
        {
            userId = userId ?? "anonymous";
            var isTraceEnabled = Logger.IsTraceEnabled();
            int count = 0;

            // Modifying an entity (also removing an entity from an aggregate) should leave the aggregate root as modified
            dbContext.ChangeTracker
                     .Entries<Entity>()
                     .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                     .Where(entry => !(entry.Entity is AggregateRoot))
                     .ForAll(entry =>
                     {
                         EntityEntry aggregateRootEntry = FindAggregateRootEntry(dbContext.ChangeTracker, entry);
                         if (aggregateRootEntry.State == EntityState.Unchanged)
                         {
                             aggregateRootEntry.State = EntityState.Modified;
                         }
                     });

            dbContext.ChangeTracker
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
                            if (isTraceEnabled)
                            {
                                Logger.Trace("tracking that {0}[{1}] was created by {2} at {3:T} UTC", entity.GetType().Name, entity.Id, userId, utcNow);
                            }
                            entity.SetCreatedProperties(userId, utcNow);
                        }
                        else if (entry.State == EntityState.Modified)
                        {
                            if (isTraceEnabled)
                            {
                                Logger.Trace("tracking that {0}[{1}] was modified by {2} at {3:T} UTC", entity.GetType().Name, entity.Id, userId, utcNow);
                            }
                            entity.SetModifiedProperties(userId, utcNow);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Updating tracking properties failed");
                        throw;
                    }
                });
            if (count > 0)
            {
                Logger.Debug($"Tracked {count} entities as created/changed on {utcNow:u} by {userId}");
            }
        }

        /// <summary>
        /// This method finds the EntityEntry&lt;AggregateRoot&gt; of an EntityEntry&lt;Entity&gt;
        /// assuming it has been loaded and is being tracked by the change tracker.
        /// </summary>
        private static EntityEntry FindAggregateRootEntry(ChangeTracker changeTracker, EntityEntry entry)
        {
            foreach (var navigation in entry.Navigations)
            {
                var navTargetTypeInfo = navigation.Metadata.GetTargetType().ClrType.GetTypeInfo();
                int navigationTargetForeignKeyValue;

                if (navigation.CurrentValue == null)
                {
                    // orphaned entity, original value contains the foreign key value
                    if (navigation.Metadata.ForeignKey.Properties.Count > 1)
                    {
                        throw new InvalidOperationException("Foreign Keys with multiple properties are not supported.");
                    }

                    IProperty property = navigation.Metadata.ForeignKey.Properties[0];
                    navigationTargetForeignKeyValue = (int)entry.OriginalValues[property];
                }
                else
                {
                    // added or modified entity, current value contains the foreign key value
                    navigationTargetForeignKeyValue = ((Entity)navigation.CurrentValue).Id;
                }

                // assumption: an entity cannot be loaded on its own. Everything on the navigation path starting from the 
                // aggregate root must have been loaded before, therefore we can find it using the change tracker
                EntityEntry<Entity> navigationTargetEntry = changeTracker
                        .Entries<Entity>()
                        .Single(ent => Equals(ent.Entity.GetType().GetTypeInfo(), navTargetTypeInfo)
                                       && ent.Property(nameof(Entity.Id)).CurrentValue.Equals(navigationTargetForeignKeyValue));

                // if the target is AggregateRoot, no (further) recursion is needed
                if (typeof(AggregateRoot).GetTypeInfo().IsAssignableFrom(navTargetTypeInfo))
                {
                    return navigationTargetEntry;
                }

                // recurse in case of "Entity -> Entity -> AggregateRoot"
                return FindAggregateRootEntry(changeTracker, navigationTargetEntry);
            }

            return null;
        }

        public static void TraceChangeTrackerState(this DbContext dbContext)
        {
            if (Logger.IsTraceEnabled())
            {
                try
                {
                    var added = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added).ToArray();
                    var modified = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified).ToArray();
                    var deleted = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Deleted).ToArray();
                    var unchanged = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Unchanged).ToArray();

                    var stateDumpBuilder = new StringBuilder();
                    stateDumpBuilder.AppendFormat("{0} entities added{1}{2}", added.Length, deleted.Length == 0 ? "." : ":", Environment.NewLine);
                    foreach (var entry in added)
                    {
                        stateDumpBuilder.AppendFormat("added: {0}[{1}]{2}", entry.Entity.GetType().Name, GetPrimaryKeyValue(entry), Environment.NewLine);
                    }
                    stateDumpBuilder.AppendFormat("{0} entities modified{1}{2}", modified.Length, deleted.Length == 0 ? "." : ":", Environment.NewLine);
                    foreach (var entry in modified)
                    {
                        stateDumpBuilder.AppendFormat("modified: {0}[{1}]{2}", entry.Entity.GetType().Name, GetPrimaryKeyValue(entry), Environment.NewLine);
                    }
                    stateDumpBuilder.AppendFormat("{0} entities deleted{1}{2}", deleted.Length, deleted.Length == 0 ? "." : ":", Environment.NewLine);
                    foreach (var entry in deleted)
                    {
                        stateDumpBuilder.AppendFormat("deleted: {0}[{1}]{2}", entry.Entity.GetType().Name, GetPrimaryKeyValue(entry), Environment.NewLine);
                    }
                    stateDumpBuilder.AppendFormat("{0} entities unchanged{1}{2}", unchanged.Length, deleted.Length == 0 ? "." : ":", Environment.NewLine);
                    foreach (var entry in unchanged)
                    {
                        stateDumpBuilder.AppendFormat("unchanged: {0}[{1}]{2}", entry.Entity.GetType().Name, GetPrimaryKeyValue(entry), Environment.NewLine);
                    }
                    Logger.Trace(stateDumpBuilder.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Change tracker state could not be dumped");
                }
            }
        }

        public static TDbContext CreateDbContext<TDbContext>(this DbContextOptions options) where TDbContext : DbContext
        {
            return (TDbContext)Activator.CreateInstance(typeof(TDbContext), options);
        }

        private static string GetPrimaryKeyValue(EntityEntry entry)
        {
            return (entry.Entity as Entity)?.Id.ToString(CultureInfo.InvariantCulture) ?? "?";
        }
    }
}
