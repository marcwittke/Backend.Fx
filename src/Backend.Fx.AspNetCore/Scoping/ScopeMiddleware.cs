namespace Backend.Fx.AspNetCore.Scoping
{
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Bootstrapping;
    using Environment.MultiTenancy;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    ///     This middleware enables use of an application runtime for each request. It makes sure that every request
    ///     is handled inside a unique execution scope resulting in a specific resolution root throughout the request.
    ///     The Middleware is responsible for beginning and completing (or disposing) the unit of work for each request.
    /// </summary>
    public abstract class ScopeMiddleware
    {
        private static readonly ILogger Logger = LogManager.Create<ScopeMiddleware>();
        private readonly RequestDelegate next;
        private readonly IBackendFxApplication application;

        [UsedImplicitly]
        protected ScopeMiddleware(RequestDelegate next, IBackendFxApplication application)
        {
            this.next = next;
            this.application = application;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            while(!application.IsBooted.Wait(3000))
            {
                Logger.Info("Queuing Request while application is booting...");
            }

            TenantId tenantId = GetTenantId(context);
            using (application.ScopeManager.BeginScope(context.User.Identity, tenantId))
            {
                 await next.Invoke(context);
            }
        }

        protected abstract TenantId GetTenantId(HttpContext context);
    }
}
