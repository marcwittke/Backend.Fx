using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Scoping
{
    public abstract class HoldCurrentMiddleware<T> where T : class
    {
        private readonly RequestDelegate _next;
        private readonly IBackendFxApplication _application;

        [UsedImplicitly]
        protected HoldCurrentMiddleware(RequestDelegate next, IBackendFxApplication application)
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
            T current = GetCurrent(context);
            var tenantIdHolder = _application.CompositionRoot.GetInstance<ICurrentTHolder<T>>();
            tenantIdHolder.ReplaceCurrent(current);
            await _next.Invoke(context);
        }

        protected abstract T GetCurrent(HttpContext context);
    }
}
