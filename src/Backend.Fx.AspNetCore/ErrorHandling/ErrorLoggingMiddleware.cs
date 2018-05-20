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
        private readonly IExceptionLogger exceptionLogger = new ExceptionLogger(LogManager.Create<ErrorLoggingMiddleware>());

        [UsedImplicitly]
        public ErrorLoggingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception uex)
            {
                if (!context.Items.ContainsKey("ExceptionLogged")) 
                {
                    exceptionLogger.LogException(uex);
                    context.Items["ExceptionLogged"] = true;
                }
                throw;
            }
        }
    }
}
