using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;

namespace Backend.Fx.Environment.MultiTenancy
{
    /// <summary>
    /// A tenant manager that keeps all tenants in a lookup held in memory. Reload is done
    /// automatically when changes occur. However, in a multi process environment, manual
    /// triggering of reload is required, when another process is updating the tenants.
    /// </summary>
    public abstract class LookupTenantManager : TenantManager
    {
        private LookupItem[] _lookupItems;
        private TenantId _defaultTenantId = new TenantId(null);

        public void EnsureLoaded()
        {
            if (_lookupItems == null) Reload();
        }

        public void Reload()
        {
            List<LookupItem> items = new List<LookupItem>();
            foreach (var tenant in LoadTenants())
            {
                var tenantId = new TenantId(tenant.Id);
                if (tenant.IsDefault)
                {
                    Interlocked.Exchange(ref _defaultTenantId, tenantId);
                }

                items.Add(new LookupItem(tenant));
            }

            var newLookupItems = items.ToArray();
            Interlocked.Exchange(ref _lookupItems, newLookupItems);
        }

        public TenantId DefaultTenantId => _defaultTenantId;


        public override TenantId[] GetTenantIds()
        {
            EnsureLoaded();
            return _lookupItems.Select(itm => itm.TenantId).ToArray();
        }

        public override Tenant[] GetTenants()
        {
            EnsureLoaded();
            return _lookupItems.Select(itm => itm.Tenant).ToArray();
        }

        public override Tenant GetTenant(TenantId tenantId)
        {
            EnsureLoaded();
            return _lookupItems.First(itm => itm.TenantId.Value == tenantId.Value).Tenant;
        }

        [CanBeNull]
        public override Tenant FindTenant(TenantId tenantId)
        {
            EnsureLoaded();
            return _lookupItems.FirstOrDefault(itm => itm.Tenant.Id == tenantId.Value)?.Tenant;
        }

        public override TenantId FindMatchingTenantId(string requestUri)
        {
            EnsureLoaded();
            return _lookupItems.FirstOrDefault(t => t.Regex != null && t.Regex.IsMatch(requestUri))?.TenantId ?? DefaultTenantId;
        }

        public override TenantId GetDefaultTenantId()
        {
            EnsureLoaded();
            return DefaultTenantId;
        }

        protected abstract Tenant[] LoadTenants();

        protected override void Dispose(bool disposing)
        { }

        private class LookupItem
        {
            public LookupItem(Tenant tenant)
            {
                Regex = tenant.UriMatchingExpression == null ? null : new Regex(tenant.UriMatchingExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                Tenant = tenant;
                TenantId = new TenantId(Tenant.Id);
            }

            public Regex Regex { get; }
            public Tenant Tenant { get; }
            public TenantId TenantId { get; }
        }
    }
}
