using System;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class AllTenantBackendFxApplicationInvoker
    {
        private readonly ITenantService _tenantService;
        private readonly IBackendFxApplicationInvoker _invoker;

        public AllTenantBackendFxApplicationInvoker(ITenantService tenantService, IBackendFxApplicationInvoker invoker)
        {
            _tenantService = tenantService;
            _invoker = invoker;
        }

        public void Invoke(Action<IInstanceProvider> action)
        {
            var correlationId = Guid.NewGuid();
            foreach (TenantId tenantId in _tenantService.GetActiveTenantIds())
            {
                _invoker.Invoke(action, new SystemIdentity(), tenantId, correlationId);
            }
        }
    }
}