using System;
using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public class WaitForBootInvoker : IBackendFxApplicationInvoker
    {
        private readonly IBackendFxApplication _application;
        private readonly IBackendFxApplicationInvoker _invoker;

        public WaitForBootInvoker(IBackendFxApplication application, IBackendFxApplicationInvoker invoker)
        {
            _application = application;
            _invoker = invoker;
        }

        public void Invoke(Action<IInstanceProvider> action, IIdentity identity, TenantId tenantId, Guid? correlationId = null)
        {
            _application.WaitForBoot();
            _invoker.Invoke(action, identity, tenantId, correlationId);
        }
    }
}