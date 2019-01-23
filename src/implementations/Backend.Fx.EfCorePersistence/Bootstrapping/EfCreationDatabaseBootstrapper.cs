using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public class EfCreationDatabaseBootstrapper<TDbContext> : EfDatabaseBootstrapper<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<EfCreationDatabaseBootstrapper<TDbContext>>();

        public EfCreationDatabaseBootstrapper(IScopeManager scopeManager) : base(scopeManager)
        { }

        protected override void ExecuteCreationStrategy(DbContext dbContext)
        {
            Logger.Info("Creating database using the current schema version. This database won't be migratable.");
            dbContext.Database.EnsureCreated();
        }
    }
}