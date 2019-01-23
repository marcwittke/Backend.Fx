namespace Backend.Fx.EfCorePersistence
{
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Backend.Fx.Patterns.DependencyInjection;

    public class DatabaseManagerWithoutMigration<TDbContext> : DatabaseManager<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManagerWithoutMigration<TDbContext>>();

        public DatabaseManagerWithoutMigration(ICompositionRoot compositionRoot) : base(compositionRoot)
        { }

        protected override void ExecuteCreationStrategy(DbContext dbContext)
        {
            Logger.Info("Creating database using the current schema version. This database won't be migratable.");
            dbContext.Database.EnsureCreated();
        }
    }
}