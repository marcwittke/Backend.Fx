namespace Backend.Fx.EfCorePersistence
{
    using Environment.Persistence;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManager<TDbContext> : IDatabaseManager where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManager<TDbContext>>();

        public DatabaseManager(DbContextOptions dbContextOptions)
        {
            DbContextOptions = dbContextOptions;
        }

        public bool DatabaseExists { get; protected set; }

        protected DbContextOptions DbContextOptions { get; }


        public virtual void EnsureDatabaseExistence()
        {
            using (var dbContext = DbContextOptions.CreateDbContext<TDbContext>())
            {
                Logger.Info("Database is being created, if not present already");
                dbContext.Database.Migrate();
            }

            DatabaseExists = true;
        }

        public virtual void DeleteDatabase()
        {
            using (var dbContext = DbContextOptions.CreateDbContext<TDbContext>())
            {
                Logger.Warn("Database is being deleted!");
                dbContext.Database.EnsureDeleted();
            }

            DatabaseExists = false;
        }
    }
}
