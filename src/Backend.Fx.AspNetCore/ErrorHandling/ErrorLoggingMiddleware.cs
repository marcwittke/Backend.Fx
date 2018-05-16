namespace Backend.Fx.AspNetCore.ErrorHandling
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.AspNetCore.Http;

    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private static readonly ILogger Logger = LogManager.Create<ErrorLoggingMiddleware>();

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
            catch (ClientException cex)
            {
                string[] clientErrorStrings = cex.Errors
                                                 .SelectMany(err => err.Value.Select(er => $"{Environment.NewLine}  {er.Code}:{er.Message}"))
                                                 .ToArray();

                var clientErrorString = string.Join("", clientErrorStrings);
                Logger.Warn(cex, cex.Message + clientErrorString);
                throw;
            }
            catch (Exception uex)
            {
                Logger.Error(uex);
                throw;
            }
        }
    }
}
