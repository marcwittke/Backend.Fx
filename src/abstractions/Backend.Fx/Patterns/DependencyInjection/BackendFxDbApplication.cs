using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;

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
        /// <param name="exceptionLogger">The exception logger used by jobs and integration event handling</param>
        protected BackendFxDbApplication(
                          ICompositionRoot compositionRoot,
                          IDatabaseBootstrapper databaseBootstrapper,
                          ITenantManager tenantManager,
                          IExceptionLogger exceptionLogger) 
            : base(compositionRoot, tenantManager, exceptionLogger)
        {
            DatabaseBootstrapper = databaseBootstrapper;
        }

        /// <summary>
        /// The utility instance for database bootstrapping
        /// </summary>
        public IDatabaseBootstrapper DatabaseBootstrapper { get; }

        protected sealed override async Task OnBoot()
        {
            await OnDatabaseBoot();
            WaitForDatabase();
            DatabaseBootstrapper.EnsureDatabaseExistence();
            await OnDatabaseBooted();
        }

        protected virtual void WaitForDatabase() { }

        /// <summary>
        /// Extension point to do additional initialization before existence of database is ensured
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnDatabaseBoot()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Extension point to do additional initialization after existence of database is ensured
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnDatabaseBooted()
        {
            await Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                DatabaseBootstrapper?.Dispose();
            }
        }
    }
}