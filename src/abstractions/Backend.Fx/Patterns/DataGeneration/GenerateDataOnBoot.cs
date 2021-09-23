using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using JetBrains.Annotations;

namespace Backend.Fx.Patterns.DataGeneration
{
    /// <summary>
    /// Enriches the <see cref="IBackendFxApplication" /> by calling all data generators for all tenants on application start.
    /// </summary>
    public class GenerateDataOnBoot : IBackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<GenerateDataOnBoot>();
        private readonly IBackendFxApplication _application;
        private readonly ManualResetEventSlim _dataGenerated = new ManualResetEventSlim(false);
        private readonly ITenantIdProvider _tenantIdProvider;

        public GenerateDataOnBoot(ITenantIdProvider tenantIdProvider, IBackendFxApplication application)
        {
            _tenantIdProvider = tenantIdProvider;
            _application = application;
            DataGenerationContext = new DataGenerationContext(
                _application.CompositionRoot,
                _application.Invoker);
        }

        public IDataGenerationContext DataGenerationContext { get; [UsedImplicitly] private set; }

        public void Dispose()
        {
            _application.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public IMessageBus MessageBus => _application.MessageBus;

        public async Task BootAsync(CancellationToken cancellationToken = default)
        {
            await _application.BootAsync(cancellationToken);

            SeedDataForAllActiveTenants();

            _dataGenerated.Set();
        }

        private void SeedDataForAllActiveTenants()
        {
            using (Logger.InfoDuration("Seeding data"))
            {
                TenantId[] prodTenantIds = _tenantIdProvider.GetActiveProductionTenantIds();
                foreach (var prodTenantId in prodTenantIds)
                {
                    DataGenerationContext.SeedDataForTenant(prodTenantId, false);
                    _application.MessageBus.Publish(new DataGenerated());
                }

                TenantId[] demoTenantIds = _tenantIdProvider.GetActiveDemonstrationTenantIds();
                foreach (var demoTenantId in demoTenantIds)
                {
                    DataGenerationContext.SeedDataForTenant(demoTenantId, true);
                    _application.MessageBus.Publish(new DataGenerated());
                }
            }
        }
    }
}
