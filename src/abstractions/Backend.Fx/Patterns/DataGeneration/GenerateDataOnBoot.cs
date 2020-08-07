using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Patterns.DataGeneration
{
    public class GenerateDataOnBoot : IBackendFxApplication
    {
        private readonly IBackendFxApplication _application;
        private readonly DataGenerationModule _dataGenerationModule;
        private readonly DataGenerationContext _dataGenerationContext;

        public GenerateDataOnBoot(IBackendFxApplication application, ITenantService tenantService, DataGenerationModule dataGenerationModule)
        {
            _application = application;
            _dataGenerationModule = dataGenerationModule;
            _dataGenerationContext = new DataGenerationContext(tenantService,
                                                               _application.CompositionRoot,
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
        
        public bool WaitForBoot(int timeoutMilliSeconds = Int32.MaxValue, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _application.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public async Task Boot(CancellationToken cancellationToken = default(CancellationToken))
        {
            _application.CompositionRoot.RegisterModules(_dataGenerationModule);
            await _application.Boot(cancellationToken);
            _dataGenerationContext.SeedDataForAllActiveTenants();
        }
    }
}