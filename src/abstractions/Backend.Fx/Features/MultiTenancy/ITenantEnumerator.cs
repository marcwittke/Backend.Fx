using System.Collections.Generic;
using JetBrains.Annotations;

namespace Backend.Fx.Features.MultiTenancy
{
    [PublicAPI]
    public interface ITenantEnumerator
    {
        IEnumerable<TenantId> GetActiveDemoTenantIds();
        IEnumerable<TenantId> GetActiveProductiveTenantIds();
        IEnumerable<TenantId> GetActiveTenantIds();
    }
}