using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public interface IBackendFxApplicationAsyncInvoker
    {
        /// <param name="awaitableAsyncAction">The async action to be invoked by the application</param>
        /// <param name="identity">The acting identity</param>
        /// <param name="tenantId">The targeted tenant id</param>
        /// <param name="correlationId">The correlation id, when it was continued</param>
        Task InvokeAsync(Func<IInstanceProvider, Task> awaitableAsyncAction, IIdentity identity, TenantId tenantId, Guid? correlationId = null);
    }


    public interface IBackendFxApplicationInvoker
    {
        /// <param name="action">The action to be invoked by the application</param>
        /// <param name="identity">The acting identity</param>
        /// <param name="tenantId">The targeted tenant id</param>
        /// <param name="correlationId">The correlation id, when it was continued</param>
        void Invoke(Action<IInstanceProvider> action, IIdentity identity, TenantId tenantId, Guid? correlationId = null);
    }


    public class BackendFxApplicationInvoker : IBackendFxApplicationInvoker, IBackendFxApplicationAsyncInvoker
    {
        private readonly ICompositionRoot _compositionRoot;
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationInvoker>();

        public BackendFxApplicationInvoker(ICompositionRoot compositionRoot)
        {
            _compositionRoot = compositionRoot;
        }


        public void Invoke(Action<IInstanceProvider> action, IIdentity identity, TenantId tenantId, Guid? correlationId = null)
        {
            Logger.LogInformation("Invoking synchronous action as {Identity} in {TenantId}", identity, tenantId);
            using (IInjectionScope injectionScope = BeginScope(identity, tenantId, correlationId))
            {
                using (UseDurationLogger(injectionScope))
                {
                    var operation = injectionScope.InstanceProvider.GetInstance<IOperation>();
                    try
                    {
                        operation.Begin();
                        action.Invoke(injectionScope.InstanceProvider);
                        injectionScope.InstanceProvider.GetInstance<IDomainEventAggregator>().RaiseEvents();
                        operation.Complete();
                    }
                    catch
                    {
                        operation.Cancel();
                        throw;
                    }

                    var messageBusScope = injectionScope.InstanceProvider.GetInstance<IMessageBusScope>();
                    AsyncHelper.RunSync(() => messageBusScope.RaiseEvents());
                }
            }
        }

        public async Task InvokeAsync(Func<IInstanceProvider, Task> awaitableAsyncAction, IIdentity identity, TenantId tenantId, Guid? correlationId = null)
        {
            Logger.LogInformation("Invoking asynchronous action as {Identity} in {TenantId}", identity, tenantId);
            using (IInjectionScope injectionScope = BeginScope(identity, tenantId, correlationId))
            {
                using (UseDurationLogger(injectionScope))
                {
                    var operation = injectionScope.InstanceProvider.GetInstance<IOperation>();
                    try
                    {
                        operation.Begin();
                        await awaitableAsyncAction.Invoke(injectionScope.InstanceProvider);
                        injectionScope.InstanceProvider.GetInstance<IDomainEventAggregator>().RaiseEvents();
                        operation.Complete();
                    }
                    catch
                    {
                        operation.Cancel();
                        throw;
                    }

                    await injectionScope.InstanceProvider.GetInstance<IMessageBusScope>().RaiseEvents();
                }
            }
        }


        private IInjectionScope BeginScope(IIdentity identity, TenantId tenantId, Guid? correlationId)
        {
            IInjectionScope injectionScope = _compositionRoot.BeginScope();
            tenantId = tenantId ?? new TenantId(null);
            injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<TenantId>>().ReplaceCurrent(tenantId);

            identity = identity ?? new AnonymousIdentity();
            injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<IIdentity>>().ReplaceCurrent(identity);

            if (correlationId.HasValue)
            {
                injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<Correlation>>().Current.Resume(correlationId.Value);
            }

            return injectionScope;
        }


        private static IDisposable UseDurationLogger(IInjectionScope injectionScope)
        {
            IIdentity identity = injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<IIdentity>>().Current;
            TenantId tenantId = injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<TenantId>>().Current;
            Correlation correlation = injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<Correlation>>().Current;
            return Logger.LogInformationDuration(
                $"Starting scope {injectionScope.SequenceNumber} (correlation [{correlation.Id}]) for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}",
                $"Ended scope {injectionScope.SequenceNumber} (correlation [{correlation.Id}]) for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}");
        }
    }
}