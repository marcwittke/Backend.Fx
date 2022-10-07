using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;

namespace Backend.Fx.ExecutionPipeline
{
    public class ExceptionLoggingAndHandlingInvoker : IBackendFxApplicationInvoker
    {
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IBackendFxApplicationInvoker _invoker;

        public ExceptionLoggingAndHandlingInvoker(IExceptionLogger exceptionLogger, IBackendFxApplicationInvoker invoker)
        {
            _exceptionLogger = exceptionLogger;
            _invoker = invoker;
        }

        public async Task InvokeAsync(Func<IServiceProvider, CancellationToken, Task> awaitableAsyncAction,
            IIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _invoker.InvokeAsync(awaitableAsyncAction, identity, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogException(ex);
            }
        }
        
        public Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity = null)
            => InvokeAsync((sp, _) => awaitableAsyncAction.Invoke(sp), identity);
    }
}