using System.Collections.Generic;

namespace Backend.Fx.Features.MultiTenancy
{
    public interface ITenantEnumerator
    {
        IEnumerable<TenantId> GetActiveDemoTenantIds();
        IEnumerable<TenantId> GetActiveProductiveTenantIds();
        IEnumerable<TenantId> GetActiveTenantIds();
    }
}