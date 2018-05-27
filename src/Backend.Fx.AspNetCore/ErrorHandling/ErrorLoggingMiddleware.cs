namespace Backend.Fx.AspNetCore.ErrorHandling
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.AspNetCore.Http;

    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IExceptionLogger exceptionLogger;

        [UsedImplicitly]
        public ErrorLoggingMiddleware(RequestDelegate next, IExceptionLogger exceptionLogger)
        {
            this.next = next;
            this.exceptionLogger = exceptionLogger;
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception exception)
            {
                if (!context.Items.ContainsKey("ExceptionLogged")) 
                {
                    exceptionLogger.LogException(exception);
                    context.Items["ExceptionLogged"] = true;
                }
                throw;
            }
        }
    }
}
