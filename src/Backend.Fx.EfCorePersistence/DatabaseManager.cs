namespace Backend.Fx.EfCorePersistence
{
    using System;
    using Environment.Persistence;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManager<TDbContext> : IDatabaseManager where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManager<TDbContext>>();

        public DatabaseManager(Func<TDbContext> dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
        }

        public bool DatabaseExists { get; protected set; }

        protected Func<TDbContext> DbContextFactory { get; }


        public virtual void EnsureDatabaseExistence()
        {
            using (var dbContext = DbContextFactory.Invoke())
            {
                Logger.Info("Database is being created, if not present already");
                dbContext.Database.Migrate();
            }

            DatabaseExists = true;
        }

        public virtual void DeleteDatabase()
        {
            using (var dbContext = DbContextFactory.Invoke())
            {
                Logger.Warn("Database is being deleted!");
                dbContext.Database.EnsureDeleted();
            }

            DatabaseExists = false;
        }
    }

    public class DatabaseManagerWithoutMigration<TDbContext> : DatabaseManager<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManagerWithoutMigration<TDbContext>>();

        public DatabaseManagerWithoutMigration(Func<TDbContext> dbContextFactory) : base(dbContextFactory)
        { }

        public override void EnsureDatabaseExistence()
        {
            using (var dbContext = DbContextFactory.Invoke())
            {
                Logger.Info("Database is being created, if not present already");
                dbContext.Database.EnsureCreated();
            }

            DatabaseExists = true;
        }
    }
}
