using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public class EfMigrationDatabaseBootstrapper<TDbContext> : EfDatabaseBootstrapper<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<EfMigrationDatabaseBootstrapper<TDbContext>>();

        public EfMigrationDatabaseBootstrapper(IScopeManager scopeManager) : base(scopeManager)
        { }
        
        protected override void ExecuteCreationStrategy(DbContext dbContext)
        {
            Logger.Info("Migrating database to latest schema version");
            dbContext.Database.Migrate();
        }
    }
}