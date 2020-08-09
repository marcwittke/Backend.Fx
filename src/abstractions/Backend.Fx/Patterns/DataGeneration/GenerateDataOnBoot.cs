using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using JetBrains.Annotations;

namespace Backend.Fx.Patterns.DataGeneration
{
    public class GenerateDataOnBoot : IBackendFxApplication
    {
        private readonly IBackendFxApplication _application;
        private readonly IModule _dataGenerationModule;
        public IDataGenerationContext DataGenerationContext { get; [UsedImplicitly] private set; }

        public GenerateDataOnBoot(ITenantService tenantService, IModule dataGenerationModule, IBackendFxApplication application)
        {
            _application = application;
            _dataGenerationModule = dataGenerationModule;
            DataGenerationContext = new DataGenerationContext(tenantService,
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

        public bool WaitForBoot(int timeoutMilliSeconds = Int32.MaxValue, CancellationToken cancellationToken = default)
        {
            return _application.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public async Task Boot(CancellationToken cancellationToken = default)
        {
            _application.CompositionRoot.RegisterModules(_dataGenerationModule);
            await _application.Boot(cancellationToken);
            DataGenerationContext.SeedDataForAllActiveTenants();
        }
    }
}