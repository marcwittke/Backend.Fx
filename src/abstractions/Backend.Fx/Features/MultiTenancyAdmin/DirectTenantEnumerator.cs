using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Features.MultiTenancy;

namespace Backend.Fx.Features.MultiTenancyAdmin
{
    public class DirectTenantEnumerator : ITenantEnumerator
    {
        private readonly ITenantRepository _tenantRepository;

        public DirectTenantEnumerator(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }
        public IEnumerable<TenantId> GetActiveDemoTenantIds()
        {
            return _tenantRepository.GetTenants().Where(t => t.IsActive && t.IsDemoTenant).Select(t => t.TenantId).ToArray();
        }

        public IEnumerable<TenantId> GetActiveProductiveTenantIds()
        {
            return _tenantRepository.GetTenants().Where(t => t.IsActive && !t.IsDemoTenant).Select(t => t.TenantId).ToArray();
        }

        public IEnumerable<TenantId> GetActiveTenantIds()
        {
            return _tenantRepository.GetTenants().Where(t => t.IsActive).Select(t => t.TenantId).ToArray();
        }
    }
}