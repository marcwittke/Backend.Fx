using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.EfCorePersistence
{
    using System;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManagerWithMigration<TDbContext> : DatabaseManager<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManagerWithMigration<TDbContext>>();

        public DatabaseManagerWithMigration(ICompositionRoot compositionRoot) : base(compositionRoot)
        { }
        
        protected override void ExecuteCreationStrategy(DbContext dbContext)
        {
            Logger.Info("Migrating database to latest schema version");
            dbContext.Database.Migrate();
        }
    }
}