using System.Collections.Generic;

namespace Backend.Fx.Environment.MultiTenancy
{
    public interface ITenantMatcherCache
    {
        void Reload(IEnumerable<Tenant> tenants);
        TenantId DefaultTenantId { get; }
        TenantId FindMatchingTenant(string requestUrl);
    }
}