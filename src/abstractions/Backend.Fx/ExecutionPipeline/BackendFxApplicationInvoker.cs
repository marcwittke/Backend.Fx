using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.ExecutionPipeline
{
    public interface IBackendFxApplicationInvoker
    {
        /// <summary>
        /// Run a delegate through the full execution pipeline, having its separate injection scope 
        /// </summary>
        /// <param name="awaitableAsyncAction">The async action to be invoked by the application</param>
        /// <param name="identity">The acting identity</param>
        /// <param name="cancellationToken">Pass an existing cancellation token (e.g. HttpContext.RequestAborted) to
        /// enable cancellation of the async invocation.</param>
        /// <returns>The <see cref="Task"/> representing the async invocation.</returns>
        Task InvokeAsync(
            Func<IServiceProvider, CancellationToken, Task> awaitableAsyncAction,
            IIdentity identity = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Run a delegate through the full execution pipeline, having its separate injection scope 
        /// </summary>
        /// <param name="awaitableAsyncAction">The async action to be invoked by the application</param>
        /// <param name="identity">The acting identity</param>
        /// <returns>The <see cref="Task"/> representing the async invocation.</returns>
        Task InvokeAsync(
            Func<IServiceProvider, Task> awaitableAsyncAction,
            IIdentity identity = null);
    }


    internal class BackendFxApplicationInvoker : IBackendFxApplicationInvoker
    {
        private readonly IBackendFxApplication _application;
        private readonly ILogger _logger = Log.Create<BackendFxApplicationInvoker>();

        public BackendFxApplicationInvoker(IBackendFxApplication application)
        {
            _application = application;
        }

        public async Task InvokeAsync(
            Func<IServiceProvider, CancellationToken, Task> awaitableAsyncAction,
            IIdentity identity = null,
            CancellationToken cancellationToken = default)
        {
            identity ??= new AnonymousIdentity();
            _logger.LogInformation("Invoking action as {Identity}", identity.Name);
            using IServiceScope serviceScope = BeginScope(identity);
            using IDisposable durationLogger = UseDurationLogger(serviceScope);
            var operation = serviceScope.ServiceProvider.GetRequiredService<IOperation>();

            try
            {
                await operation.BeginAsync(serviceScope, cancellationToken).ConfigureAwait(false);
                await awaitableAsyncAction.Invoke(serviceScope.ServiceProvider, cancellationToken).ConfigureAwait(false);
                await operation.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await operation.CancelAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity = null)
            => InvokeAsync((sp, _) => awaitableAsyncAction.Invoke(sp), identity);

        private IServiceScope BeginScope(IIdentity identity)
        {
            IServiceScope serviceScope = _application.CompositionRoot.BeginScope();

            identity ??= new AnonymousIdentity();
            serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<IIdentity>>().ReplaceCurrent(identity);

            return serviceScope;
        }

        private IDisposable UseDurationLogger(IServiceScope serviceScope)
        {
            IIdentity identity = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<IIdentity>>().Current;
            Correlation correlation = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<Correlation>>().Current;
            return _logger.LogInformationDuration(
                $"Starting invocation (correlation [{correlation.Id}]) for {identity.Name}",
                $"Ended invocation (correlation [{correlation.Id}]) for {identity.Name}");
        }
    }
}