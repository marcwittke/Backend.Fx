using Backend.Fx.AspNetCore.MultiTenancy;
using Backend.Fx.Environment.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    public class MultiTenantMiddleware : MultiTenantMiddlewareBase
    {
        public MultiTenantMiddleware(RequestDelegate next) : base(next)
        { }

        protected override TenantId FindMatchingTenantId(HttpContext context)
        {
            if (context.Request.Query.TryGetValue("tenantId", out StringValues tenantIdStr)
                && int.TryParse(tenantIdStr[0], out int tenantId))
            {
                return new TenantId(tenantId);
            }

            return new TenantId(null);
        }
    }
}