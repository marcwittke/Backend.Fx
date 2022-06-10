using System.Linq;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class SingleTenantApplication
    {
        private static readonly ILogger Logger = Log.Create<SingleTenantApplication>();
        private readonly ITenantService _tenantService;
        private readonly bool _singleTenantIsDemoTenant;
        private readonly object _padlock = new object();

        public SingleTenantApplication(
            IMessageBus messageBus,
            ITenantRepository tenantRepository,
            bool singleTenantIsDemoTenant)
        {
            _tenantService = new TenantService(tenantRepository);
            _singleTenantIsDemoTenant = singleTenantIsDemoTenant;
        }

        public TenantId TenantId { get; private set; }

        public ITenantIdProvider TenantProvider => _tenantService.TenantIdProvider;

        public void Boot()
        {
            lock (_padlock)
            {
                Logger.Info("Ensuring existence of single tenant");
                TenantId = _tenantService.GetActiveTenants().SingleOrDefault()?.GetTenantId()
                           ?? _tenantService.CreateTenant("Single Tenant",
                               "This application runs in single tenant mode",
                               _singleTenantIsDemoTenant);
            }
        }
    }
}