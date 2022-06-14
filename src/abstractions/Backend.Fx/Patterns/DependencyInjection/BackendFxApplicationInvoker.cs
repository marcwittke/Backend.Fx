using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Microsoft.Extensions.DependencyInjection;
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
        Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity, TenantId tenantId, Guid? correlationId = null);
    }


    public interface IBackendFxApplicationInvoker
    {
        /// <param name="action">The action to be invoked by the application</param>
        /// <param name="identity">The acting identity</param>
        /// <param name="tenantId">The targeted tenant id</param>
        /// <param name="correlationId">The correlation id, when it was continued</param>
        void Invoke(Action<IServiceProvider> action, IIdentity identity, TenantId tenantId, Guid? correlationId = null);
    }


    public class BackendFxApplicationInvoker : IBackendFxApplicationInvoker, IBackendFxApplicationAsyncInvoker
    {
        private readonly ICompositionRoot _compositionRoot;
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationInvoker>();

        public BackendFxApplicationInvoker(ICompositionRoot compositionRoot)
        {
            _compositionRoot = compositionRoot;
        }


        public void Invoke(Action<IServiceProvider> action, IIdentity identity, TenantId tenantId, Guid? correlationId = null)
        {
            Logger.LogInformation("Invoking synchronous action as {Identity} in {TenantId}", identity, tenantId);
            using (IServiceScope serviceScope = BeginScope(identity, tenantId, correlationId))
            {
                using (UseDurationLogger(serviceScope))
                {
                    var operation = serviceScope.ServiceProvider.GetRequiredService<IOperation>();
                    try
                    {
                        operation.Begin();
                        action.Invoke(serviceScope.ServiceProvider);
                        serviceScope.ServiceProvider.GetRequiredService<IDomainEventAggregator>().RaiseEvents();
                        operation.Complete();
                    }
                    catch
                    {
                        operation.Cancel();
                        throw;
                    }
                }
            }
        }

        public async Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity, TenantId tenantId, Guid? correlationId = null)
        {
            Logger.LogInformation("Invoking asynchronous action as {Identity} in {TenantId}", identity, tenantId);
            using (IServiceScope serviceScope = BeginScope(identity, tenantId, correlationId))
            {
                using (UseDurationLogger(serviceScope))
                {
                    var operation = serviceScope.ServiceProvider.GetRequiredService<IOperation>();
                    try
                    {
                        operation.Begin();
                        await awaitableAsyncAction.Invoke(serviceScope.ServiceProvider).ConfigureAwait(false);
                        serviceScope.ServiceProvider.GetRequiredService<IDomainEventAggregator>().RaiseEvents();
                        operation.Complete();
                    }
                    catch
                    {
                        operation.Cancel();
                        throw;
                    }
                }
            }
        }


        private IServiceScope BeginScope(IIdentity identity, TenantId tenantId, Guid? correlationId)
        {
            IServiceScope serviceScope = _compositionRoot.BeginScope();
            tenantId = tenantId ?? new TenantId(null);
            serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<TenantId>>().ReplaceCurrent(tenantId);

            identity = identity ?? new AnonymousIdentity();
            serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<IIdentity>>().ReplaceCurrent(identity);

            if (correlationId.HasValue)
            {
                serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<Correlation>>().Current.Resume(correlationId.Value);
            }

            return serviceScope;
        }


        private static IDisposable UseDurationLogger(IServiceScope serviceScope)
        {
            IIdentity identity = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<IIdentity>>().Current;
            TenantId tenantId = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<TenantId>>().Current;
            Correlation correlation = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<Correlation>>().Current;
            return Logger.LogInformationDuration(
                $"Starting scope (correlation [{correlation.Id}]) for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}",
                $"Ended scope (correlation [{correlation.Id}]) for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}");
        }
    }
}