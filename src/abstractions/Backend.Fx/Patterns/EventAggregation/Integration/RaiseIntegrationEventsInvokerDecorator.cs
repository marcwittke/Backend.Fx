using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class RaiseIntegrationEventsInvokerDecorator : IBackendFxApplicationInvoker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBackendFxApplicationInvoker _invoker;

        public RaiseIntegrationEventsInvokerDecorator(
            IServiceProvider serviceProvider,
            IBackendFxApplicationInvoker invoker)
        {
            _serviceProvider = serviceProvider;
            _invoker = invoker;
        }

        public void Invoke(
            Action<IServiceProvider> action, 
            IIdentity identity,
            TenantId tenantId,
            Guid? correlationId = null)
        {
            _invoker.Invoke(action, identity, tenantId, correlationId);
            AsyncHelper.RunSync(() => _serviceProvider.GetRequiredService<IMessageBusScope>().RaiseEvents());
        }
    }

    public class RaiseIntegrationEventsAsyncInvokerDecorator : IBackendFxApplicationAsyncInvoker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBackendFxApplicationAsyncInvoker _invoker;

        public RaiseIntegrationEventsAsyncInvokerDecorator(
            IServiceProvider serviceProvider,
            IBackendFxApplicationAsyncInvoker invoker)
        {
            _serviceProvider = serviceProvider;
            _invoker = invoker;
        }

        public async Task InvokeAsync(
            Func<IServiceProvider, Task> awaitableAsyncAction,
            IIdentity identity,
            TenantId tenantId,
            Guid? correlationId = null)
        {
            await _invoker.InvokeAsync(awaitableAsyncAction, identity, tenantId, correlationId).ConfigureAwait(false);
            await _serviceProvider.GetRequiredService<IMessageBusScope>().RaiseEvents().ConfigureAwait(false);
        }
    }
}