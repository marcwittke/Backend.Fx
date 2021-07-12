using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.MultiTenancy
{
    public abstract class MultiTenantMiddlewareBase
    {
        private readonly RequestDelegate _next;

        protected MultiTenantMiddlewareBase(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task Invoke(HttpContext context)
        {
            context.SetCurrentTenantId(FindMatchingTenantId(context));
            await _next.Invoke(context);
        }
 
        /// <summary>
        /// Detects the <see cref="TenantId"/> for this request from the current HttpContext. Possible implementations might rely on
        /// a dedicated header value, the (sub-) domain name, a query string parameter etc. This method is called for each request. If
        /// the database is required for determination, some kind of caching is advised.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The TenantId for this request</returns>
        protected abstract TenantId FindMatchingTenantId(HttpContext context);
    }
}