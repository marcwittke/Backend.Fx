namespace Backend.Fx.EfCorePersistence
{
    using System;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManagerWithoutMigration<TDbContext> : DatabaseManager<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManagerWithoutMigration<TDbContext>>();

        public DatabaseManagerWithoutMigration(Func<TDbContext> dbContextFactory) : base(dbContextFactory)
        { }

        protected override void ExecuteCreationStrategy(DbContext dbContext)
        {
            Logger.Info("Creating database using the current schema version. This database won't be migratable.");
            dbContext.Database.EnsureCreated();
        }
    }
}