namespace DemoBlog.Mvc.Infrastructure
{
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using Backend.Fx.Environment.MultiTenancy;
    using Backend.Fx.Patterns.DependencyInjection;
    using Data.Identity;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;

    public class DemoBlogMiddleware : ScopeMiddleware
    {
        public DemoBlogMiddleware(RequestDelegate next, IScopeManager scopeManager, IHostingEnvironment env) : base(next, scopeManager, env)
        { }

        protected override TenantId GetTenantId(IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.Claims.SingleOrDefault(cl => cl.Type == BlogUser.TenantIdClaimType);
            int parsed;
            if (claim != null && int.TryParse(claim.Value, out parsed))
            {
                return new TenantId(parsed);
            }

            return new TenantId(null);
        }
    }
}
