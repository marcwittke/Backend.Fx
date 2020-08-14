using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using JetBrains.Annotations;

namespace Backend.Fx.Patterns.DataGeneration
{
    public class GenerateDataOnBoot : IBackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<GenerateDataOnBoot>();
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IBackendFxApplication _application;
        private readonly IModule _dataGenerationModule;
        public IDataGenerationContext DataGenerationContext { get; [UsedImplicitly] private set; }

        public GenerateDataOnBoot(ITenantIdProvider tenantIdProvider, IModule dataGenerationModule, IBackendFxApplication application)
        {
            _tenantIdProvider = tenantIdProvider;
            _application = application;
            _dataGenerationModule = dataGenerationModule;
            DataGenerationContext = new DataGenerationContext(_application.CompositionRoot,
                                                              _application.Invoker);
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

        public async Task Boot(CancellationToken cancellationToken = default)
        {
            _application.CompositionRoot.RegisterModules(_dataGenerationModule);
            await _application.Boot(cancellationToken);
            
            
            SeedDataForAllActiveTenants();
        }
        
        public void SeedDataForAllActiveTenants()
        {
            using (Logger.InfoDuration("Seeding data"))
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