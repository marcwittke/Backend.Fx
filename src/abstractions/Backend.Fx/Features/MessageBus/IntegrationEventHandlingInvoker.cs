using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;

namespace Backend.Fx.Features.MessageBus
{
    /// <summary>
    /// Ensures integration events to be handled sequentially
    /// </summary>
    public class IntegrationEventHandlingInvoker : IBackendFxApplicationInvoker
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly IBackendFxApplicationInvoker _invoker;

        public IntegrationEventHandlingInvoker(IBackendFxApplicationInvoker invoker)
        {
            _invoker = invoker;
        }


        public async Task InvokeAsync(Func<IServiceProvider, CancellationToken, Task> awaitableAsyncAction,
            IIdentity identity, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _invoker
                    .InvokeAsync(awaitableAsyncAction, identity, cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity = null)
            => InvokeAsync((sp, _) => awaitableAsyncAction.Invoke(sp), identity);
    }
}