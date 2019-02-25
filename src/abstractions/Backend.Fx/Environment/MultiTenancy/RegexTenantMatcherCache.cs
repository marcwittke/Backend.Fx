using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class RegexTenantMatcherCache : ITenantMatcherCache
    {
        private CacheItem[] _items;

        public void Reload(IEnumerable<Tenant> tenants)
        {
            List<CacheItem> items = new List<CacheItem>();
            foreach (var tenant in tenants)
            {
                if (tenant.State != TenantState.Inactive && !string.IsNullOrEmpty(tenant.UriMatchingExpression))
                {
                    var tenantId = new TenantId(tenant.Id);
                    if (tenant.IsDefault)
                    {
                        DefaultTenantId = tenantId;
                    }

                    items.Add(new CacheItem(new Regex(tenant.UriMatchingExpression, RegexOptions.Compiled), tenantId));
                }
            }

            _items = items.ToArray();
        }

        public TenantId DefaultTenantId { get; private set; } = new TenantId(null);

        public TenantId FindMatchingTenant(string requestUrl)
        {
            return _items.FirstOrDefault(t => t.Regex.IsMatch(requestUrl))?.TenantId ?? DefaultTenantId;
        }

        private class CacheItem
        {
            public CacheItem(Regex regex, TenantId tenantId)
            {
                Regex = regex;
                TenantId = tenantId;
            }

            public Regex Regex { get; }
            public TenantId TenantId { get; }
        }
    }
}
