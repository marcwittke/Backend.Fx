using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DataGeneration
{
    /// <summary>
    /// Enriches the <see cref="IBackendFxApplication"/> by calling all data generators for all tenants
    /// on application start
    /// </summary>
    public class DataGeneratingApplication : BackendFxApplicationDecorator
    {
        private static readonly ILogger Logger = Log.Create<DataGeneratingApplication>();

        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IBackendFxApplication _application;
        
        public IDataGenerationContext DataGenerationContext { get; [UsedImplicitly] private set; }

        /// <param name="tenantIdProvider">To be able to query all active demo/production tenants</param>
        /// <param name="tenantWideMutexManager">to make sure data generation will never run in parallel for the same tenant</param>
        /// <param name="application">the decorated instance</param>
        public DataGeneratingApplication(
            ITenantIdProvider tenantIdProvider, 
            ITenantWideMutexManager tenantWideMutexManager,
            IBackendFxApplication application) : base(application)
        {
            _tenantIdProvider = tenantIdProvider;
            _application = application;
            DataGenerationContext = new DataGenerationContext(
                _application.CompositionRoot,
                _application.Invoker,
                tenantWideMutexManager);
        }

        public override async Task BootAsync(CancellationToken cancellationToken = default)
        {
            _application.CompositionRoot.RegisterModules(new DataGenerationModule(_application.Assemblies));
            await base.BootAsync(cancellationToken).ConfigureAwait(false);
            SeedDataForAllActiveTenants();
        }

        private void SeedDataForAllActiveTenants()
        {
            using (Logger.LogInformationDuration("Seeding data"))
            {
                var prodTenantIds = _tenantIdProvider.GetActiveProductionTenantIds();
                foreach (TenantId prodTenantId in prodTenantIds)
                {
                    DataGenerationContext.SeedDataForTenant(prodTenantId, false);
                }

                var demoTenantIds = _tenantIdProvider.GetActiveDemonstrationTenantIds();
                foreach (TenantId demoTenantId in demoTenantIds)
                {
                    DataGenerationContext.SeedDataForTenant(demoTenantId, true);
                }
            }
        }
    }
}