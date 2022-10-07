using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;

namespace Backend.Fx.Features.MessageBus
{
    /// <summary>
    /// Ensures events to be handled sequentially
    /// </summary>
    public class IntegrationEventHandlingInvoker : IBackendFxApplicationInvoker
    {
        private readonly object _syncLock = new();
        private readonly IBackendFxApplicationInvoker _invoker;

        public IntegrationEventHandlingInvoker(IBackendFxApplicationInvoker invoker)
        {
            _invoker = invoker;
        }


        public async Task InvokeAsync(Func<IServiceProvider, CancellationToken, Task> awaitableAsyncAction,
            IIdentity identity, CancellationToken cancellationToken = default)
        {
            Monitor.Enter(_syncLock);
            try
            {
                await _invoker
                    .InvokeAsync(awaitableAsyncAction, identity, cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                Monitor.Exit(_syncLock);
            }
        }

        public Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity = null)
            => InvokeAsync((sp, _) => awaitableAsyncAction.Invoke(sp), identity);
    }
}