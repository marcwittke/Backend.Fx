using System;
using System.Linq;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class AllTenantBackendFxApplicationInvoker
    {
        private static readonly ILogger Logger = Log.Create<AllTenantBackendFxApplicationInvoker>();
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IBackendFxApplicationInvoker _invoker;

        public AllTenantBackendFxApplicationInvoker(ITenantIdProvider tenantIdProvider, IBackendFxApplicationInvoker invoker)
        {
            _tenantIdProvider = tenantIdProvider;
            _invoker = invoker;
        }

        public void Invoke(Action<IInstanceProvider> action)
        {
            var correlationId = Guid.NewGuid();
            TenantId[] tenantIds = _tenantIdProvider.GetActiveDemonstrationTenantIds().Concat(_tenantIdProvider.GetActiveProductionTenantIds()).ToArray();
            Logger.LogDebug("Action will be called in tenants: {TenantIds}", string.Join(",", tenantIds.Select(t => t.ToString())));
            foreach (TenantId tenantId in tenantIds)
            {
                _invoker.Invoke(action, new SystemIdentity(), tenantId, correlationId);
            }
        }
    }
}