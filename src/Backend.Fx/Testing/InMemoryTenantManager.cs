namespace Backend.Fx.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Environment.MultiTenancy;
    using Extensions;

    public class InMemoryTenantManager : TenantManager
    {
        [ThreadStatic]
        private static Dictionary<int, Tenant> store;
        
        public InMemoryTenantManager(ITenantInitializer tenantInitializer) : base(tenantInitializer)
        {
            store = store ?? new Dictionary<int, Tenant>();
        }

        public override TenantId[] GetTenantIds()
        {
            return store.Keys.Select(id => new TenantId(id)).ToArray();
        }

        public override Tenant[] GetTenants()
        {
            return store.Values.ToArray();
        }

        protected override Tenant FindTenant(TenantId tenantId)
        {
            return store[tenantId.Value];
        }

        protected override void SaveTenant(Tenant tenant)
        {
            if (tenant.Id == 0)
            {
                tenant.Id = store.Any() ? store.Keys.Max() + 1 : 1;
            }

            store[tenant.Id] = tenant;

            if (tenant.IsDefault)
            {
                store.Values.Where(t => t.Id != tenant.Id).ForAll(t => t.IsDefault = false);
            }
        }
    }
}
