using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Scoping
{
    /// <summary>
    ///     This middleware enables use of an application scope for each request. It makes sure that every request
    ///     is handled inside a unique execution scope resulting in a specific resolution root throughout the request.
    /// </summary>
    public class ScopeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IBackendFxApplication _application;

        [UsedImplicitly]
        public ScopeMiddleware(RequestDelegate next, IBackendFxApplication application)
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
            using (_application.BeginScope())
            {
                await _next.Invoke(context);
            }
        }
    }
}
