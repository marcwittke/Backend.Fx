using System.Threading.Tasks;
using Backend.Fx.AspNetCore.Mvc;
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
            context.Items[HttpContextItemKey.TenantId] = FindMatchingTenantId(context);
            await _next.Invoke(context);
        }
        
        protected abstract TenantId FindMatchingTenantId(HttpContext context);
    }
}