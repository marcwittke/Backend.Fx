using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Logging;

namespace Backend.Fx.ExecutionPipeline
{
    internal class ExceptionLoggingInvoker : IBackendFxApplicationInvoker
    {
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IBackendFxApplicationInvoker _invoker;

        public ExceptionLoggingInvoker(IExceptionLogger exceptionLogger, IBackendFxApplicationInvoker invoker)
        {
            _exceptionLogger = exceptionLogger;
            _invoker = invoker;
        }

        public async Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity)
        {
            try
            {
                await _invoker.InvokeAsync(awaitableAsyncAction, identity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogException(ex);
                throw;
            }
        }
    }
}