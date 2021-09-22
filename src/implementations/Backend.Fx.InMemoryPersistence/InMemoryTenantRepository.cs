using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Environment.MultiTenancy;

namespace Backend.Fx.InMemoryPersistence
{
    public class InMemoryTenantRepository : ITenantRepository
    {
        private readonly Dictionary<int, Tenant> _store = new Dictionary<int, Tenant>();

        public Tenant[] GetTenants()
        {
            return _store.Values.ToArray();
        }

        public Tenant GetTenant(TenantId tenantId)
        {
            return _store[tenantId.Value];
        }

        public void DeleteTenant(TenantId tenantId)
        {
            _store.Remove(tenantId.Value);
        }

        public void SaveTenant(Tenant tenant)
        {
            if (tenant.Id == 0)
            {
                tenant.Id = _store.Any() ? _store.Keys.Max() + 1 : 1;
                _store[tenant.Id] = tenant;
            }
            else
            {
                _store[tenant.Id].Description = tenant.Description;
                _store[tenant.Id].Configuration = tenant.Configuration;
                _store[tenant.Id].IsDemoTenant = tenant.IsDemoTenant;
                _store[tenant.Id].Name = tenant.Name;
                _store[tenant.Id].State = tenant.State;
            }
        }
    }
}
