using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Backend.Fx.EfCorePersistence
{
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

        public static void ApplyAggregateMappings(this DbContext dbContext, ModelBuilder modelBuilder)
        {
            //CAVE: IAggregateMapping implementations must reside in the same assembly as the Applications DbContext-type
            IEnumerable<TypeInfo> aggregateDefinitionTypeInfos = dbContext
                .GetType()
                .GetTypeInfo()
                .Assembly
                .ExportedTypes
                .Select(t => t.GetTypeInfo())
                .Where(
                    t => t.IsClass && !t.IsAbstract && !t.IsGenericType &&
                        typeof(IAggregateMapping).GetTypeInfo().IsAssignableFrom(t));
            foreach (var typeInfo in aggregateDefinitionTypeInfos)
            {
                var aggregateMapping = (IAggregateMapping)Activator.CreateInstance(typeInfo.AsType());
                aggregateMapping.ApplyEfMapping(modelBuilder);
            }
        }

        public static void TraceChangeTrackerState(this DbContext dbContext)
        {
            if (Logger.IsTraceEnabled())
            {
                try
                {
                    EntityEntry[] added = dbContext.ChangeTracker.Entries()
                        .Where(entry => entry.State == EntityState.Added)
                        .ToArray();
                    EntityEntry[] modified = dbContext.ChangeTracker.Entries()
                        .Where(entry => entry.State == EntityState.Modified)
                        .ToArray();
                    EntityEntry[] deleted = dbContext.ChangeTracker.Entries()
                        .Where(entry => entry.State == EntityState.Deleted)
                        .ToArray();
                    EntityEntry[] unchanged = dbContext.ChangeTracker.Entries()
                        .Where(entry => entry.State == EntityState.Unchanged)
                        .ToArray();

                    var stateDumpBuilder = new StringBuilder();
                    stateDumpBuilder.AppendFormat(
                        "{0} entities added{1}{2}",
                        added.Length,
                        deleted.Length == 0 ? "." : ":",
                        System.Environment.NewLine);
                    foreach (var entry in added)
                    {
                        stateDumpBuilder.AppendFormat(
                            "added: {0}[{1}]{2}",
                            entry.Entity.GetType().Name,
                            GetPrimaryKeyValue(entry),
                            System.Environment.NewLine);
                    }

                    stateDumpBuilder.AppendFormat(
                        "{0} entities modified{1}{2}",
                        modified.Length,
                        deleted.Length == 0 ? "." : ":",
                        System.Environment.NewLine);
                    foreach (var entry in modified)
                    {
                        stateDumpBuilder.AppendFormat(
                            "modified: {0}[{1}]{2}",
                            entry.Entity.GetType().Name,
                            GetPrimaryKeyValue(entry),
                            System.Environment.NewLine);
                    }

                    stateDumpBuilder.AppendFormat(
                        "{0} entities deleted{1}{2}",
                        deleted.Length,
                        deleted.Length == 0 ? "." : ":",
                        System.Environment.NewLine);
                    foreach (var entry in deleted)
                    {
                        stateDumpBuilder.AppendFormat(
                            "deleted: {0}[{1}]{2}",
                            entry.Entity.GetType().Name,
                            GetPrimaryKeyValue(entry),
                            System.Environment.NewLine);
                    }

                    stateDumpBuilder.AppendFormat(
                        "{0} entities unchanged{1}{2}",
                        unchanged.Length,
                        deleted.Length == 0 ? "." : ":",
                        System.Environment.NewLine);
                    foreach (var entry in unchanged)
                    {
                        stateDumpBuilder.AppendFormat(
                            "unchanged: {0}[{1}]{2}",
                            entry.Entity.GetType().Name,
                            GetPrimaryKeyValue(entry),
                            System.Environment.NewLine);
                    }

                    Logger.Trace(stateDumpBuilder.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Change tracker state could not be dumped");
                }
            }
        }

        private static string GetPrimaryKeyValue(EntityEntry entry)
        {
            return (entry.Entity as Entity)?.Id.ToString(CultureInfo.InvariantCulture) ?? "?";
        }
    }
}
