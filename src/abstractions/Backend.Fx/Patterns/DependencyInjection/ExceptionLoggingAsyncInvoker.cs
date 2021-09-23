using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public class ExceptionLoggingAsyncInvoker : IBackendFxApplicationAsyncInvoker
    {
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IBackendFxApplicationAsyncInvoker _invoker;

        public ExceptionLoggingAsyncInvoker(IExceptionLogger exceptionLogger, IBackendFxApplicationAsyncInvoker invoker)
        {
            _exceptionLogger = exceptionLogger;
            _invoker = invoker;
        }

        public async Task InvokeAsync(
            Func<IInstanceProvider, Task> awaitableAsyncAction,
            IIdentity identity,
            TenantId tenantId,
            Guid? correlationId = null)
        {
            try
            {
                await _invoker.InvokeAsync(awaitableAsyncAction, identity, tenantId, correlationId);
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogException(ex);
                throw;
            }
        }
    }
}
