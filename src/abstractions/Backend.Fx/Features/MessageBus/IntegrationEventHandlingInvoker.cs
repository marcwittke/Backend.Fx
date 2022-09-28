using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;

namespace Backend.Fx.Features.MessageBus
{
    /// <summary>
    /// Ensures events to be handled sequentially and catches all exceptions.
    /// </summary>
    public class IntegrationEventHandlingInvoker : IBackendFxApplicationInvoker
    {
        private readonly object _syncLock = new();
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IBackendFxApplicationInvoker _invoker;

        public IntegrationEventHandlingInvoker(IExceptionLogger exceptionLogger, IBackendFxApplicationInvoker invoker)
        {
            _exceptionLogger = exceptionLogger;
            _invoker = invoker;
        }


        public async Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity)
        {
            Monitor.Enter(_syncLock);
            try
            {
                await _invoker.InvokeAsync(
                    awaitableAsyncAction,
                    identity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogException(ex);
            }
            finally
            {
                Monitor.Exit(_syncLock);
            }
        }
    }
}