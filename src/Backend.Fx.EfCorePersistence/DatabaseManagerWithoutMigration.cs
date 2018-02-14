namespace Backend.Fx.EfCorePersistence
{
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManagerWithoutMigration<TDbContext> : DatabaseManager<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManagerWithoutMigration<TDbContext>>();

        public DatabaseManagerWithoutMigration(DbContextOptions<TDbContext> options) : base(options)
        { }

        protected override void ExecuteCreationStrategy(DbContext dbContext)
        {
            Logger.Info("Creating database using the current schema version. This database won't be migratable.");
            dbContext.Database.EnsureCreated();
        }
    }
}