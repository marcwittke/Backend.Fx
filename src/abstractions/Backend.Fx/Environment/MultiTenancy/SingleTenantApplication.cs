using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class SingleTenantApplication : TenantApplication, IBackendFxApplication
    {
        private readonly ITenantService _tenantService;
        private readonly IBackendFxApplication _application;
        private readonly TenantCreationParameters _tenantCreationParameters;

        public SingleTenantApplication(TenantCreationParameters tenantCreationParameters, ITenantService tenantService, IBackendFxApplication application) : base(application)
        {
            _tenantService = tenantService;
            _application = application;
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

        public bool WaitForBoot(int timeoutMilliSeconds = Int32.MaxValue, CancellationToken cancellationToken = default)
        {
            return _application.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public async Task Boot(CancellationToken cancellationToken = default)
        {
            EnableDataGenerationForNewTenants();

            await _application.Boot(cancellationToken);

            TenantId = _tenantService.GetActiveTenantIds().SingleOrDefault()
                       ?? _tenantService.CreateTenant(_tenantCreationParameters);
        }
    }
}