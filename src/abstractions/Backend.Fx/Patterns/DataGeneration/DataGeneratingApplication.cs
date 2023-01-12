using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DataGeneration
{
    /// <summary>
    /// Enriches the <see cref="IBackendFxApplication"/> by calling all data generators for all tenants
    /// on application start and when a tenant gets activated
    /// </summary>
    public class DataGeneratingApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = Log.Create<DataGeneratingApplication>();

        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IBackendFxApplication _application;
        private readonly IModule _dataGenerationModule;

        public IDataGenerationContext DataGenerationContext { get; [UsedImplicitly] private set; }

        /// <param name="tenantIdProvider">To be able to query all active demo/production tenants</param>
        /// <param name="dataGenerationModule">To register the collection of <code>IDataGenerator</code> with the composition root. Internally, <code>IInstanceProvider.GetInstances&lt;IDataGenerator&gt;()</code> is being used</param>
        /// <param name="tenantWideMutexManager">to make sure data generation will never run in parallel for the same tenant</param>
        /// <param name="application">the decorated instance</param>
        public DataGeneratingApplication(
            ITenantIdProvider tenantIdProvider, 
            IModule dataGenerationModule,
            ITenantWideMutexManager tenantWideMutexManager,
            IBackendFxApplication application)
        {
            _tenantIdProvider = tenantIdProvider;
            _application = application;
            _dataGenerationModule = dataGenerationModule;
            DataGenerationContext = new DataGenerationContext(
                _application.CompositionRoot,
                _application.Invoker,
                tenantWideMutexManager);
        }

        public void Dispose()
        {
            _application.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;
        public ICompositionRoot CompositionRoot => _application.CompositionRoot;
        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public IMessageBus MessageBus => _application.MessageBus;

        public bool WaitForBoot(int timeoutMilliSeconds = Int32.MaxValue, CancellationToken cancellationToken = default)
        {
            return _application.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public Task Boot(CancellationToken cancellationToken = default) => BootAsync(cancellationToken);

        public async Task BootAsync(CancellationToken cancellationToken = default)
        {
            _application.CompositionRoot.RegisterModules(_dataGenerationModule);
            EnableDataGenerationForNewTenants();

            await _application.BootAsync(cancellationToken);

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
                    var dataGenerated = new DataGenerated();
                    dataGenerated.SetTenantId(prodTenantId);
                    _application.MessageBus.Publish(dataGenerated);
                }

                var demoTenantIds = _tenantIdProvider.GetActiveDemonstrationTenantIds();
                foreach (TenantId demoTenantId in demoTenantIds)
                {
                    DataGenerationContext.SeedDataForTenant(demoTenantId, true);
                    var dataGenerated = new DataGenerated();
                    dataGenerated.SetTenantId(demoTenantId);
                    _application.MessageBus.Publish(dataGenerated);
                }
            }
        }

        private void EnableDataGenerationForNewTenants()
        {
            _application.MessageBus.Subscribe(new DelegateIntegrationMessageHandler<TenantActivated>(tenantActivated =>
            {
                Logger.LogInformation(
                    "Seeding data for recently activated tenant (with demo data: {IsDemoTenant}) {TenantId}",
                    tenantActivated.IsDemoTenant,
                    tenantActivated.TenantId);
                try
                {
                    DataGenerationContext.SeedDataForTenant(
                        new TenantId(tenantActivated.TenantId),
                        tenantActivated.IsDemoTenant);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,
                        "Seeding data for recently activated tenant (with demo data: {IsDemoTenant}) {TenantId} failed",
                        tenantActivated.IsDemoTenant,
                        tenantActivated.TenantId);
                }
            }));
        }
    }
}