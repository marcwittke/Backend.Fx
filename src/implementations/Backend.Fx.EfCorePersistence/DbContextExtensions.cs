using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.EfCorePersistence
{
    public static class DbContextExtensions
    {
        private static readonly ILogger Logger = Log.Create(typeof(DbContextExtensions));

        public static void DisableChangeTracking(this DbContext dbContext)
        {
            Logger.LogDebug("Disabling change tracking on {DbContextTypeName} instance", dbContext.GetType().Name);
            dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public static void RegisterRowVersionProperty(this ModelBuilder modelBuilder)
        {
            modelBuilder.Model
                        .GetEntityTypes()
                        .Where(mt => typeof(Entity).GetTypeInfo().IsAssignableFrom(mt.GetClrType().GetTypeInfo()))
                        .ForAll(mt => modelBuilder.Entity(mt.GetClrType()).Property<byte[]>("RowVersion").IsRowVersion());
        }

        public static void RegisterEntityIdAsNeverGenerated(this ModelBuilder modelBuilder)
        {
            modelBuilder.Model
                        .GetEntityTypes()
                        .Where(mt => typeof(Entity).GetTypeInfo().IsAssignableFrom(mt.GetClrType().GetTypeInfo()))
                        .ForAll(mt => modelBuilder.Entity(mt.GetClrType()).Property(nameof(Entity.Id)).ValueGeneratedNever());
        }

        public static void ApplyAggregateMappings(this DbContext dbContext, ModelBuilder modelBuilder)
        {
            //CAVE: IAggregateMapping implementations must reside in the same assembly as the Applications DbContext-type
            var aggregateDefinitionTypeInfos = dbContext
                                               .GetType()
                                               .GetTypeInfo()
                                               .Assembly
                                               .ExportedTypes
                                               .Select(t => t.GetTypeInfo())
                                               .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(IAggregateMapping).GetTypeInfo().IsAssignableFrom(t));
            foreach (TypeInfo typeInfo in aggregateDefinitionTypeInfos)
            {
                var aggregateMapping = (IAggregateMapping) Activator.CreateInstance(typeInfo.AsType());
                aggregateMapping.ApplyEfMapping(modelBuilder);
            }
        }

        

        public static void TraceChangeTrackerState(this DbContext dbContext)
        {
            if (Logger.IsEnabled(LogLevel.Trace))
                try
                {
                    var changeTrackerState = new
                    {
                        added = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added).ToArray(),
                        modified = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified).ToArray(),
                        deleted = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Deleted).ToArray(),
                        unchanged = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Unchanged).ToArray()
                    };

                    Logger.LogTrace("Change tracker state: {@ChangeTrackerState}", changeTrackerState);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Change tracker state could not be dumped");
                }
        }

        private static string GetPrimaryKeyValue(EntityEntry entry)
        {
            return (entry.Entity as Entity)?.Id.ToString(CultureInfo.InvariantCulture) ?? "?";
        }

        internal static Type GetClrType([NotNull] this IEntityType mt)
        {
            if (mt == null) throw new ArgumentNullException(nameof(mt));
            var propertyInfo = mt.GetType().GetProperty("ClrType");
            Debug.Assert(propertyInfo != null);
            return (Type) propertyInfo.GetValue(mt);
        }
    }
}