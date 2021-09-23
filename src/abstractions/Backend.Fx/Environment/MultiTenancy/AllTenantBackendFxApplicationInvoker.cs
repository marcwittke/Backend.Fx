using System;
using System.Linq;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class AllTenantBackendFxApplicationInvoker
    {
        private static readonly ILogger Logger = LogManager.Create<AllTenantBackendFxApplicationInvoker>();
        private readonly IBackendFxApplicationInvoker _invoker;
        private readonly ITenantIdProvider _tenantIdProvider;

        public AllTenantBackendFxApplicationInvoker(
            ITenantIdProvider tenantIdProvider,
            IBackendFxApplicationInvoker invoker)
        {
            _tenantIdProvider = tenantIdProvider;
            _invoker = invoker;
        }

        public void Invoke(Action<IInstanceProvider> action)
        {
            var correlationId = Guid.NewGuid();
            TenantId[] tenantIds = _tenantIdProvider.GetActiveDemonstrationTenantIds()
                .Concat(_tenantIdProvider.GetActiveProductionTenantIds())
                .ToArray();
            Logger.Debug($"Action will be called in tenants: {string.Join(",", tenantIds.Select(t => t.ToString()))}");
            foreach (var tenantId in tenantIds)
            {
                _invoker.Invoke(action, new SystemIdentity(), tenantId, correlationId);
            }
        }
    }
}
