using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public abstract class BackendFxDbApplication : BackendFxApplication
    {
        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="databaseBootstrapper">The database manager for the current application</param>
        /// <param name="tenantManager">The tenant manager for the current application</param>
        /// <param name="scopeManager">The scope manager for the current application</param>
        protected BackendFxDbApplication(
                          ICompositionRoot compositionRoot,
                          IDatabaseBootstrapper databaseBootstrapper,
                          ITenantManager tenantManager,
                          IScopeManager scopeManager) : base(compositionRoot, scopeManager, tenantManager)
        {
            DatabaseBootstrapper = databaseBootstrapper;
        }

        /// <summary>
        /// The utility instance for database bootstrapping
        /// </summary>
        public IDatabaseBootstrapper DatabaseBootstrapper { get; }

        protected sealed override async Task OnBoot()
        {
            WaitForDatabase();
            DatabaseBootstrapper.EnsureDatabaseExistence();
            await OnDatabaseBoot();
        }

        protected virtual void WaitForDatabase() { }

        /// <summary>
        /// Extension point to do additional initialization after existence of database is ensured
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnDatabaseBoot()
        {
            await Task.CompletedTask;
        }
    }
}