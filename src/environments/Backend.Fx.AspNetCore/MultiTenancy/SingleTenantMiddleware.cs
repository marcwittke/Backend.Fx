using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.MultiTenancy
{
    /// <summary>
    /// Always assumes TenantId: 1 for all requests.
    /// </summary>
    public class SingleTenantMiddleware
    {
        private readonly RequestDelegate _next;

        public SingleTenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public virtual async Task Invoke(HttpContext context)
        {
            context.SetCurrentTenantId(new TenantId(1));
            await _next.Invoke(context);
        }
    }
}