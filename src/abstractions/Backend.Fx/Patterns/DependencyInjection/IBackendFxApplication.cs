using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Jobs;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public interface IBackendFxApplication : IDisposable
    {
        /// <summary>
        /// The composition root of the dependency injection framework
        /// </summary>
        ICompositionRoot CompositionRoot { get; }

        /// <summary>
        /// Used to log exceptions 
        /// </summary>
        IExceptionLogger ExceptionLogger { get; }

        /// <summary>
        /// Provides application wide tenant ids
        /// </summary>
        ITenantIdService TenantIdService { get; }

        /// <summary>
        /// allows synchronously awaiting application startup
        /// </summary>
        bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Initializes ans starts the application (async)
        /// </summary>
        /// <returns></returns>
        Task Boot(CancellationToken cancellationToken = default(CancellationToken));

        IDisposable BeginScope(IIdentity identity = null, TenantId tenantId = null);

        /// <param name="action">The action to be invoked by the application</param>
        /// <param name="identity">The acting identity</param>
        /// <param name="tenantId">The targeted tenant id</param>
        /// <param name="configureScope">An optional action that is used to configure the scope before beginning the unit of work</param>
        void Invoke(Action action, IIdentity identity, TenantId tenantId, Action configureScope = null);

        /// <param name="awaitableAsyncAction">The async action to be invoked by the application</param>
        /// <param name="identity">The acting identity</param>
        /// <param name="tenantId">The targeted tenant id</param>
        /// <param name="configureScope">An optional action that is used to configure the scope before beginning the unit of work</param>
        Task InvokeAsync(Func<Task> awaitableAsyncAction, IIdentity identity, TenantId tenantId, Action configureScope = null);

        void Run<TJob>() where TJob : class, IJob;

        void Run<TJob>(TenantId tenantId) where TJob : class, IJob;
    }
}
