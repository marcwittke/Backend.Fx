using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;

namespace Backend.Fx.InMemoryPersistence
{
    public class InMemoryTenantManager : LookupTenantManager
    {
        private readonly Dictionary<int, Tenant> _store = new Dictionary<int, Tenant>();
        
        protected override Tenant[] LoadTenants()
        {
            return _store.Values.Select(Clone).ToArray();
        }

        public override Tenant GetTenant(TenantId tenantId)
        {
            return Clone(base.GetTenant(tenantId));
        }

        public override Tenant FindTenant(TenantId tenantId)
        {
            return Clone(base.FindTenant(tenantId));
        }

        protected override void Dispose(bool disposing)
        {}

        protected override void SaveTenantPersistent(Tenant existingTenant, Tenant tenant)
        {
            if (tenant.Id == 0)
            {
                tenant.Id = _store.Any() ? _store.Keys.Max() + 1 : 1;
                _store[tenant.Id] = tenant;
            }
            else
            {
                _store[tenant.Id].DefaultCultureName = tenant.DefaultCultureName;
                _store[tenant.Id].Description = tenant.Description;
                _store[tenant.Id].IsDemoTenant = tenant.IsDemoTenant;
                _store[tenant.Id].IsDefault = tenant.IsDefault;
                _store[tenant.Id].Name = tenant.Name;
                _store[tenant.Id].State = tenant.State;
            }

            if (tenant.IsDefault)
            {
                _store.Values.Where(t => t.Id != tenant.Id).ForAll(t => t.IsDefault = false);
            }
        }

        // cloning the tenants simulates the behavior of a db persistence, of not having a direct reference to the persisted object
        private static Tenant Clone(Tenant t)
        {
            if (t == null) return null;
            return new Tenant(t.Id, t.Name, t.Description, t.IsDemoTenant, t.State, t.IsDefault, t.DefaultCultureName,
                t.UriMatchingExpression);
        }
    }
}
