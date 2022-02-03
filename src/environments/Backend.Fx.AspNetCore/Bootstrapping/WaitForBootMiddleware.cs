using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.AspNetCore.Bootstrapping
{
    /// <summary>
    /// Queues all requests until the application finished booting.
    /// </summary>
    public class WaitForBootMiddleware
    {
        private static readonly ILogger Logger = LogManager.Create<WaitForBootMiddleware>();
        private readonly RequestDelegate _next;
        private readonly IBackendFxApplication _application;

        [UsedImplicitly]
        public WaitForBootMiddleware(RequestDelegate next, IBackendFxApplication application)
        {
            _next = next;
            _application = application;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            while (!_application.WaitForBoot(3000))
            {
                Logger.LogInformation("Queuing Request while application is booting...");
            }

            await _next.Invoke(context);
        }
    }
}