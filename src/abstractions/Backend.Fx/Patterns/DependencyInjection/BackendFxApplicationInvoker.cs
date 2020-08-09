using System;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.UnitOfWork;

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
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplicationInvoker>();
        private readonly IExceptionLogger _exceptionLogger;

        public BackendFxApplicationInvoker(ICompositionRoot compositionRoot, IExceptionLogger exceptionLogger)
        {
            _compositionRoot = compositionRoot;
            _exceptionLogger = exceptionLogger;
        }


        public void Invoke(Action<IInstanceProvider> action, IIdentity identity, TenantId tenantId, Guid? correlationId = null)
        {
            try
            {
                using (IInjectionScope injectionScope = _compositionRoot.BeginScope())
                {
                    MaintainScopeVariables(injectionScope, identity, tenantId, correlationId);
                    Correlation correlation = injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<Correlation>>().Current;
                    using (Logger.InfoDuration(
                        $"Starting scope {injectionScope.SequenceNumber} (correlation [{correlation.Id}]) for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}",
                        $"Ended scope {injectionScope.SequenceNumber} (correlation [{correlation.Id}]) for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}")
                    )
                    {
                        //!!!!! unit of work is not mandatory, but should be enabled via decorator
                        // how to access the instance provider ?
                        // how to hook the uow decorator after opening the scope, but before executing the action?
                        var unitOfWork = injectionScope.InstanceProvider.GetInstance<IUnitOfWork>();
                        try
                        {
                            unitOfWork.Begin();
                            action.Invoke(injectionScope.InstanceProvider);
                            unitOfWork.Complete();
                        }
                        catch (TargetInvocationException ex)
                        {
                            _exceptionLogger.LogException(ex.InnerException ?? ex);
                        }
                        catch (Exception ex)
                        {
                            Logger.Info(ex);
                            _exceptionLogger.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception);
            }
        }


        public async Task InvokeAsync(Func<IInstanceProvider, Task> awaitableAsyncAction, IIdentity identity, TenantId tenantId, Guid? correlationId = null)
        {
            try
            {
                using (IInjectionScope injectionScope = _compositionRoot.BeginScope())
                {
                    MaintainScopeVariables(injectionScope, identity, tenantId, correlationId);
                    Correlation correlation = injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<Correlation>>().Current;
                    using (Logger.InfoDuration(
                        $"Starting scope {injectionScope.SequenceNumber} (correlation [{correlation.Id}]) for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}",
                        $"Ended scope {injectionScope.SequenceNumber} (correlation [{correlation.Id}]) for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}")
                    )
                    {
                        var unitOfWork = injectionScope.InstanceProvider.GetInstance<IUnitOfWork>();
                        try
                        {
                            unitOfWork.Begin();
                            await awaitableAsyncAction.Invoke(injectionScope.InstanceProvider);
                            unitOfWork.Complete();
                        }
                        catch (TargetInvocationException ex)
                        {
                            _exceptionLogger.LogException(ex.InnerException ?? ex);
                        }
                        catch (Exception ex)
                        {
                            Logger.Info(ex);
                            _exceptionLogger.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception);
            }
        }


        private void MaintainScopeVariables(IInjectionScope injectionScope, IIdentity identity, TenantId tenantId, Guid? correlationId)
        {
            tenantId = tenantId ?? new TenantId(null);
            injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<TenantId>>().ReplaceCurrent(tenantId);

            identity = identity ?? new AnonymousIdentity();
            injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<IIdentity>>().ReplaceCurrent(identity);

            if (correlationId.HasValue)
            {
                injectionScope.InstanceProvider.GetInstance<ICurrentTHolder<Correlation>>().Current.Resume(correlationId.Value);
            }
        }
    }
}