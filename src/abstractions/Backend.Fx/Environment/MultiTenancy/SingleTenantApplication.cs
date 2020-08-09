using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class SingleTenantApplication : IBackendFxApplication
    {
        private readonly IBackendFxApplication _application;
        private readonly ITenantService _tenantService;
        private readonly IModule _multiTenancyModule;
        private readonly TenantCreationParameters _tenantCreationParameters;

        public SingleTenantApplication(IBackendFxApplication application, ITenantService tenantService, IModule multiTenancyModule,
                                       TenantCreationParameters tenantCreationParameters)
        {
            _application = application;
            _tenantService = tenantService;
            _multiTenancyModule = multiTenancyModule;
            _tenantCreationParameters = tenantCreationParameters;
        }

        public void Dispose()
        {
            _application.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public IMessageBus MessageBus => _application.MessageBus;

        public TenantId TenantId { get; private set; }

        public bool WaitForBoot(int timeoutMilliSeconds = Int32.MaxValue, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _application.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public async Task Boot(CancellationToken cancellationToken = default(CancellationToken))
        {
            _application.CompositionRoot.RegisterModules(_multiTenancyModule);

            await _application.Boot(cancellationToken);

            TenantId = _tenantService.GetActiveTenantIds().SingleOrDefault()
                       ?? _tenantService.CreateTenant(_tenantCreationParameters);
        }
    }
}