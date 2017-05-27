namespace DemoBlog.Mvc.Data.Identity
{
    using System.Globalization;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

    public sealed class BlogUser : IdentityUser
    {
        public const string TenantIdClaimType = "urn:demoblog:tenantid";

        [UsedImplicitly]
        private BlogUser()
        {}

        public BlogUser(int tenantId)
        {
            Claims.Add(new IdentityUserClaim<string> {
                ClaimType = TenantIdClaimType,
                ClaimValue = tenantId.ToString(CultureInfo.InvariantCulture),
            });
        }
    }
}
