namespace Backend.Fx.EfCorePersistence
{
    using System;
    using Environment.Persistence;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManager<TDbContext> : IDatabaseManager where TDbContext : DbContext
    {
        private readonly Func<TDbContext> dbContextFactory;
        private static readonly ILogger Logger = LogManager.Create<DatabaseManager<TDbContext>>();

        public DatabaseManager(Func<TDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public bool DatabaseExists { get; private set; }
        
        public void EnsureDatabaseExistence()
        {
            using (var dbContext = dbContextFactory.Invoke())
            {
                Logger.Info("Database is being created, if not present already");
                dbContext.Database.Migrate();
            }

            DatabaseExists = true;
        }

        public void DeleteDatabase()
        {
            using (var dbContext = dbContextFactory.Invoke())
            {
                Logger.Warn("Database is being deleted!");
                dbContext.Database.EnsureDeleted();
            }

            DatabaseExists = false;
        }
    }
}
