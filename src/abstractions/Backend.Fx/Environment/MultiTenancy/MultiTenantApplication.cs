using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class MultiTenantApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<MultiTenantApplication>();

        private readonly IBackendFxApplication _application;
        private readonly ITenantService _tenantService;
        private readonly IModule _multiTenancyModule;
        private readonly DataGenerationContext _dataGenerationContext;

        public MultiTenantApplication(IBackendFxApplication application, ITenantService tenantService, IModule multiTenancyModule)
        {
            _application = application;
            _tenantService = tenantService;
            _multiTenancyModule = multiTenancyModule;
            _dataGenerationContext = new DataGenerationContext(tenantService, _application.CompositionRoot, _application.Invoker);
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
            _application.CompositionRoot.RegisterModules(_multiTenancyModule);

            await _application.Boot(cancellationToken);

            MessageBus.Subscribe(new DelegateIntegrationMessageHandler<TenantCreated>(tenantCreated =>
            {
                Logger.Info($"Seeding data for recently created {(tenantCreated.IsDemoTenant ? "demo " : "")}tenant {tenantCreated.TenantId}");
                try
                {
                    var tenantId = new TenantId(tenantCreated.TenantId);
                    _dataGenerationContext.SeedDataForTenant(tenantId, tenantCreated.IsDemoTenant);
                    _tenantService.ActivateTenant(tenantId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Seeding data for recently created {(tenantCreated.IsDemoTenant ? "demo " : "")}tenant {tenantCreated.TenantId} failed.");
                }
            }));
        }
    }
}