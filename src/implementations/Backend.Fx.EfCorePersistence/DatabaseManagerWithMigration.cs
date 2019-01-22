namespace Backend.Fx.EfCorePersistence
{
    using System;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManagerWithMigration<TDbContext> : DatabaseManager<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManagerWithMigration<TDbContext>>();

        public DatabaseManagerWithMigration(Func<TDbContext> dbContextFactory) : base(dbContextFactory)
        { }
        
        protected override void ExecuteCreationStrategy(DbContext dbContext)
        {
            Logger.Info("Migrating database to latest schema version");
            dbContext.Database.Migrate();
        }
    }
}