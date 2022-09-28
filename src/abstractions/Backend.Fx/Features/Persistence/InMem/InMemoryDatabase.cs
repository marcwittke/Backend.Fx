using System.Collections.Concurrent;
using Backend.Fx.Features.MultiTenancy;

namespace Backend.Fx.Features.Persistence.InMem
{
    public class InMemoryDatabase
    {
        private readonly ConcurrentDictionary<TenantId, IAggregateDictionaries> _inMemoryStores = new();

        public IAggregateDictionaries GetInMemoryStores()
        {
            return _inMemoryStores.GetOrAdd(new TenantId(null), _ => new AggregateDictionaries());
        }
        
        public IAggregateDictionaries GetInMemoryStoresOfTenant(TenantId tenantId)
        {
            return _inMemoryStores.GetOrAdd(tenantId, _ => new AggregateDictionaries());
        }
    }
}