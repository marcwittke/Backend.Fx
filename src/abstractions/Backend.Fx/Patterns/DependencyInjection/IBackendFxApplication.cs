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
        bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes ans starts the application (async)
        /// </summary>
        /// <returns></returns>
        Task Boot(CancellationToken cancellationToken = default);

        IDisposable BeginScope(IIdentity identity = null, TenantId tenantId = null);

        Task InvokeAsync(Action action, IIdentity identity, TenantId tenantId);

        Task InvokeAsync(Func<Task> awaitableAsyncAction, IIdentity identity, TenantId tenantId);

        Task RunAsync<TJob>() where TJob : class, IJob;

        Task RunAsync<TJob>(TenantId tenantId) where TJob : class, IJob;
    }
}
