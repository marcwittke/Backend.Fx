namespace DemoBlog.Mvc.Infrastructure.Middlewares
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using Backend.Fx.AspNetCore.Scoping;
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Environment.MultiTenancy;
    using Data.Identity;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Http;

    public class BlogMiddleware : ScopeMiddleware
    {
        private readonly Lazy<TenantId> defaultTenantId;

        /// <summary>
        ///     This constructor is being called by the framework DI container
        /// </summary>
        [UsedImplicitly]
        public BlogMiddleware(RequestDelegate next, IBackendFxApplication application) 
                : base(next, application)
        {
            defaultTenantId = new Lazy<TenantId>(()=> ((BackendFxDbApplication)application).TenantManager.GetDefaultTenantId());
        }

        protected override TenantId GetTenantId(HttpContext context)
        {
            ClaimsIdentity claimsIdentity = context.User.Identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.Claims.SingleOrDefault(cl => cl.Type == BlogUser.TenantIdClaimType);
            if (claim != null && int.TryParse(claim.Value, out var parsed))
            {
                return new TenantId(parsed);
            }

            return defaultTenantId.Value;
        }
    }
}