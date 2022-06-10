using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DataGeneration
{
    /// <summary>
    /// Enriches the <see cref="IBackendFxApplication"/> by calling all data generators for all tenants
    /// on application start and when a tenant gets activated
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
            //EnableDataGenerationForNewTenants();
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

        // todo refactor
        // private void EnableDataGenerationForNewTenants()
        // {
        //     _application..Subscribe(new DelegateIntegrationMessageHandler<TenantActivated>(tenantCreated =>
        //     {
        //         Logger.LogInformation(
        //             "Seeding data for recently activated tenant (with demo data: {IsDemoTenant}) {TenantId}",
        //             tenantCreated.IsDemoTenant,
        //             tenantCreated.TenantId);
        //         try
        //         {
        //             DataGenerationContext.SeedDataForTenant(new TenantId(tenantCreated.TenantId),
        //                 tenantCreated.IsDemoTenant);
        //         }
        //         catch (Exception ex)
        //         {
        //             Logger.LogError(ex,
        //                 "Seeding data for recently activated tenant (with demo data: {IsDemoTenant}) {TenantId} failed",
        //                 tenantCreated.IsDemoTenant,
        //                 tenantCreated.TenantId);
        //         }
        //     }));
        // }
    }

    public class DataGenerationModule : IModule
    {
        private readonly Assembly[] _assemblies;

        public DataGenerationModule(Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }
        
        public void Register(ICompositionRoot compositionRoot)
        {
            ServiceDescriptor[] serviceDescriptors = _assemblies
                .GetImplementingTypes(typeof(IDataGenerator))
                .Select(t => new ServiceDescriptor(typeof(IDataGenerator), t, ServiceLifetime.Scoped))
                .ToArray();
            
            compositionRoot.RegisterServiceDescriptors(serviceDescriptors);
        }
    }
}