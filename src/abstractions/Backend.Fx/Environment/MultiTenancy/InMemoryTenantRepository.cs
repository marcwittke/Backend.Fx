using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class InMemoryTenantRepository : ITenantRepository
    {
        private readonly Dictionary<int, Tenant> _store = new Dictionary<int, Tenant>();

        public InMemoryTenantRepository()
        { }

        /// <summary>
        /// If your tenants are kept as a file that is loaded on application startup, you can initialize this instance
        /// using this constructor.
        /// </summary>
        /// <param name="tenants"></param>
        public InMemoryTenantRepository([NotNull] params Tenant[] tenants)
        {
            if (tenants == null)
            {
                throw new ArgumentNullException(nameof(tenants));
            }

            _store = tenants.ToDictionary(t => t.Id, t => t);
        }

        /// <summary>
        /// If your tenants are kept as a file that is loaded on application startup, you can initialize this instance
        /// using this constructor.
        /// </summary>
        /// <param name="tenants"></param>
        public InMemoryTenantRepository([NotNull] IEnumerable<Tenant> tenants) : this(tenants.ToArray())
        { }

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
