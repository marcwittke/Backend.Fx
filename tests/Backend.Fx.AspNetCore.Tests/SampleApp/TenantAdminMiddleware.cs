using Backend.Fx.AspNetCore.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    public class TenantAdminMiddleware : TenantAdminMiddlewareBase 
    {
        public TenantAdminMiddleware(RequestDelegate next, SampleApplicationHostedService hostedService) 
            : base(next, hostedService.TenantService)
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