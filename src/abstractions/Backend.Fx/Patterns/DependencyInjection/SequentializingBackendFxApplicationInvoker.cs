using System;
using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public class SequentializingBackendFxApplicationInvoker : IBackendFxApplicationInvoker
    {
        private readonly object _syncLock = new object();
        private readonly IBackendFxApplicationInvoker _backendFxApplicationInvokerImplementation;

        public SequentializingBackendFxApplicationInvoker(IBackendFxApplicationInvoker backendFxApplicationInvokerImplementation)
        {
            _backendFxApplicationInvokerImplementation = backendFxApplicationInvokerImplementation;
        }


        public void Invoke(Action<IInstanceProvider> action, IIdentity identity, TenantId tenantId, Guid? correlationId = null)
        {
            lock (_syncLock)
            {
                _backendFxApplicationInvokerImplementation.Invoke(action, identity, tenantId, correlationId);
            }
        }
    }
}