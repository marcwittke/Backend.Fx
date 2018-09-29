using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Jobs;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public abstract class BackendFxDbApplication : BackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxDbApplication>();

        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="databaseManager">The database manager for the current application</param>
        /// <param name="tenantManager">The tenant manager for the current application</param>
        /// <param name="scopeManager">The scope manager for the current application</param>
        protected BackendFxDbApplication(
                          ICompositionRoot compositionRoot,
                          IDatabaseManager databaseManager,
                          ITenantManager tenantManager,
                          IScopeManager scopeManager) : base(compositionRoot,scopeManager)
        {
            DatabaseManager = databaseManager;
            TenantManager = tenantManager;
        }

        /// <summary>
        /// The utility instance for database management
        /// </summary>
        public IDatabaseManager DatabaseManager { get; }

        /// <summary>
        /// Access and maintains application tenants
        /// </summary>
        public ITenantManager TenantManager { get; }

        public override async Task Boot()
        {
            await base.Boot();
            DatabaseManager.EnsureDatabaseExistence();
        }

        protected void ExecuteJob<TJob>() where TJob : IJob
        {
            foreach (var tenantId in TenantManager.GetTenantIds())
            {
                using (Logger.InfoDuration($"Execution of {typeof(TJob).Name} for tenant[{(tenantId.HasValue ? tenantId.Value.ToString() : "null")}]"))
                {
                    using (ScopeManager.BeginScope(new SystemIdentity(), tenantId))
                    {
                        CompositionRoot.GetInstance<IJobExecutor<TJob>>().ExecuteJob();
                    }
                }
            }
        }
    }
}