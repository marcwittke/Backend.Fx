using System;
using System.Collections.Concurrent;
using Backend.Fx.Features.MultiTenancy;

namespace Backend.Fx.Features.Persistence.InMem
{
    public class InMemoryDatabase<TId> where TId : struct, IEquatable<TId>
    {
        private readonly ConcurrentDictionary<TenantId, IAggregateDictionaries<TId>> _inMemoryStores = new();

        public IAggregateDictionaries<TId> GetInMemoryStores()
        {
            return _inMemoryStores.GetOrAdd(new TenantId(null), _ => new AggregateDictionaries<TId>());
        }
        
        public IAggregateDictionaries<TId> GetInMemoryStoresOfTenant(TenantId tenantId)
        {
            return _inMemoryStores.GetOrAdd(tenantId, _ => new AggregateDictionaries<TId>());
        }
    }
}