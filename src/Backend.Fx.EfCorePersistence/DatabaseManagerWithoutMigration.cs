namespace Backend.Fx.EfCorePersistence
{
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManagerWithoutMigration<TDbContext> : DatabaseManager<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManagerWithoutMigration<TDbContext>>();

        public DatabaseManagerWithoutMigration(DbContextOptions options) : base(options)
        { }

        public override void EnsureDatabaseExistence()
        {
            using (var dbContext = DbContextOptions.CreateDbContext<TDbContext>())
            {
                Logger.Info("Database is being created, if not present already");
                dbContext.Database.EnsureCreated();
            }

            DatabaseExists = true;
        }
    }
}