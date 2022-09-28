using System.Collections.Concurrent;
using System.Linq;
using Backend.Fx.Exceptions;

namespace Backend.Fx.Features.MultiTenancyAdmin.InMem
{
    public class InMemoryTenantRepository : ITenantRepository
    {
        private readonly ConcurrentDictionary<int, Tenant> _tenantsDictionary = new();

        public void SaveTenant(Tenant tenant)
        {
            _tenantsDictionary[tenant.Id] = tenant;
        }

        public Tenant[] GetTenants()
        {
            return _tenantsDictionary.Values.ToArray();
        }

        public Tenant GetTenant(int tenantId)
        {
            return _tenantsDictionary.ContainsKey(tenantId)
                ? _tenantsDictionary[tenantId]
                : throw new NotFoundException<Tenant>(tenantId);
        }

        public void DeleteTenant(int tenantId)
        {
            _tenantsDictionary.TryRemove(tenantId, out _);
        }

        public int GetNextTenantId()
        {
            if (_tenantsDictionary.IsEmpty)
            {
                return 1;
            }
            
            return _tenantsDictionary.Keys.Max() + 1;
        }
    }
}