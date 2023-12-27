using Backend.Fx.AspNetCore.MultiTenancy;
using Backend.Fx.Environment.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    public class TenantAdminMiddleware : TenantAdminMiddlewareBase 
    {
        public TenantAdminMiddleware(RequestDelegate next, ITenantService tenantService) 
            : base(next, tenantService)
        {
        }

        protected override string GetTenantConfiguration(CreateTenantParams createTenantParams)
        {
            return createTenantParams.Configuration;
        }

        protected override bool IsTenantsAdmin(HttpContext context)
        {
            return true;
        }
    }
}