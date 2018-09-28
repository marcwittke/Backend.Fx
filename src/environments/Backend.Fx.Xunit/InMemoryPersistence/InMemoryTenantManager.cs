using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;

namespace Backend.Fx.Xunit.InMemoryPersistence
{
    public class InMemoryTenantManager : TenantManager
    {
        private readonly Dictionary<int, Tenant> _store = new Dictionary<int, Tenant>();

        public InMemoryTenantManager(ITenantInitializer tenantInitializer) : base(tenantInitializer)
        {}

        public override TenantId[] GetTenantIds()
        {
            return _store.Keys.Select(id => new TenantId(id)).ToArray();
        }

        public override Tenant[] GetTenants()
        {
            return _store.Values.ToArray();
        }

        public override Tenant FindTenant(TenantId tenantId)
        {
            return _store[tenantId.Value];
        }

        protected override void SaveTenant(Tenant tenant)
        {
            if (tenant.Id == 0)
            {
                tenant.Id = _store.Any() ? _store.Keys.Max() + 1 : 1;
            }

            _store[tenant.Id] = tenant;

            if (tenant.IsDefault)
            {
                _store.Values.Where(t => t.Id != tenant.Id).ForAll(t => t.IsDefault = false);
            }
        }
    }
}
