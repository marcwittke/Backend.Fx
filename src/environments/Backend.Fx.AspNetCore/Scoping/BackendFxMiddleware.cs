using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Scoping
{
    public interface IBackendFxMiddleware : IMiddleware
    {
    }

    public abstract class BackendFxMiddleware : IBackendFxMiddleware
    {
        private readonly IBackendFxApplicationAsyncInvoker _invoker;

        protected BackendFxMiddleware(IBackendFxApplicationAsyncInvoker invoker)
        {
            _invoker = invoker;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            TenantId tenantId = FindMatchingTenantId(context);
            IIdentity identity = context.User.Identity;

            await _invoker.InvokeAsync(_ => next.Invoke(context), identity, tenantId);
        }

        protected abstract TenantId FindMatchingTenantId(HttpContext context);

        protected virtual IIdentity GetIdentity(HttpContext context)
        {
            return context.User.Identity;
        }
    }
}