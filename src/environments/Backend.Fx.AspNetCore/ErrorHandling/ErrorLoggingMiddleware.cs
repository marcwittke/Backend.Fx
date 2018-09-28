namespace Backend.Fx.AspNetCore.ErrorHandling
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.AspNetCore.Http;

    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IExceptionLogger _exceptionLogger;

        [UsedImplicitly]
        public ErrorLoggingMiddleware(RequestDelegate next, IExceptionLogger exceptionLogger)
        {
            this._next = next;
            this._exceptionLogger = exceptionLogger;
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception exception)
            {
                if (!context.Items.ContainsKey("ExceptionLogged")) 
                {
                    _exceptionLogger.LogException(exception);
                    context.Items["ExceptionLogged"] = true;
                }
                throw;
            }
        }
    }
}
