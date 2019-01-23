using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Scoping
{
    /// <summary>
    ///     This middleware enables use of an application runtime for each request. It makes sure that every request
    ///     is handled inside a unique execution scope resulting in a specific resolution root throughout the request.
    ///     The Middleware is responsible for beginning and completing (or disposing) the unit of work for each request.
    /// </summary>
    public abstract class ScopeMiddleware
    {
        private static readonly ILogger Logger = LogManager.Create<ScopeMiddleware>();
        private readonly RequestDelegate _next;
        private readonly IBackendFxApplication _application;

        [UsedImplicitly]
        protected ScopeMiddleware(RequestDelegate next, IBackendFxApplication application)
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
            while (!await _application.WaitForBootAsync(3000))
            {
                Logger.Info("Queuing Request while application is booting...");
            }

            TenantId tenantId = GetTenantId(context);
            using (_application.ScopeManager.BeginScope(context.User.Identity, tenantId))
            {
                await _next.Invoke(context);
            }
        }

        protected abstract TenantId GetTenantId(HttpContext context);
    }
}
