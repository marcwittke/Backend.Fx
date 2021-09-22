using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class SingleTenantApplication : TenantApplication, IBackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<SingleTenantApplication>();
        private readonly IBackendFxApplication _application;
        private readonly ManualResetEventSlim _defaultTenantEnsured = new ManualResetEventSlim(false);
        private readonly bool _isDemoTenant;
        private readonly ITenantService _tenantService;

        public SingleTenantApplication(
            bool isDemoTenant,
            ITenantService tenantService,
            IBackendFxApplication application) : base(application)
        {
            _isDemoTenant = isDemoTenant;
            _tenantService = tenantService;
            _application = application;
        }

        public TenantId TenantId { get; private set; }

        public void Dispose()
        {
            _application.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public IMessageBus MessageBus => _application.MessageBus;

        public bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue, CancellationToken cancellationToken = default)
        {
            return _defaultTenantEnsured.Wait(timeoutMilliSeconds, cancellationToken);
        }

        public Task Boot(CancellationToken cancellationToken = default)
        {
            return BootAsync(cancellationToken);
        }

        public async Task BootAsync(CancellationToken cancellationToken = default)
        {
            EnableDataGenerationForNewTenants();

            await _application.BootAsync(cancellationToken);

            Logger.Info("Ensuring existence of single tenant");
            TenantId = _tenantService.GetActiveTenants().SingleOrDefault()?.GetTenantId()
                ?? _tenantService.CreateTenant(
                    "Single Tenant",
                    "This application runs in single tenant mode",
                    _isDemoTenant);

            _defaultTenantEnsured.Set();
        }
    }
}
