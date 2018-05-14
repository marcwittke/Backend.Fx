namespace Backend.Fx.Bootstrapping
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using Logging;
    using Patterns.DependencyInjection;
    using Patterns.Jobs;
    
    
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
        /// <param name="jobExecutor">The job executor for the current application. If not provided, a default <see cref="Patterns.Jobs.JobExecutor"/> instance is generated.</param>
        protected BackendFxDbApplication(
                          ICompositionRoot compositionRoot,
                          IDatabaseManager databaseManager,
                          ITenantManager tenantManager,
                          IScopeManager scopeManager,
                          IJobExecutor jobExecutor = null) : base(compositionRoot,scopeManager)
        {
            JobExecutor = jobExecutor ?? new JobExecutor(tenantManager, scopeManager);
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

        public IJobExecutor JobExecutor { get; }

        public virtual async Task Boot(bool doEnsureDevelopmentTenantExistenceOnBoot)
        {
            await base.Boot();
            DatabaseManager.EnsureDatabaseExistence();

            if (doEnsureDevelopmentTenantExistenceOnBoot)
            {
                EnsureDevelopmentTenantExistence();
            }
        }

        /// <summary>
        /// This is only supported in development environments. Running inside of an IIS host will result in timeouts during the first 
        /// request, leaving the system in an unpredicted state. To achieve the same effect in a hosted demo environment, use the same
        /// functionality via service endpoints.
        /// </summary>
        public virtual void EnsureDevelopmentTenantExistence()
        {
            const string devTenantCode = "dev";
            if (DatabaseManager.DatabaseExists)
            {
                var tenants = TenantManager.GetTenants();
                if (tenants.Any(t => t.IsDemoTenant && t.Name == devTenantCode))
                {
                    return;
                }
            }

            Logger.Info("Creating dev tenant");

            // This will create a demonstration tenant. Note that by using the TenantManager directly instead of the TenantsController
            // there won't be any TenantCreated event published...
            TenantId tenantId = TenantManager.CreateDemonstrationTenant(devTenantCode, "dev tenant", true, new CultureInfo("en-US"));

            // ... therefore it's up to us to do the initialization. Which is fine, because we are not spinning of a background action
            // but blocking in our thread.
            TenantManager.EnsureTenantIsInitialized(tenantId);
        }
    }
}