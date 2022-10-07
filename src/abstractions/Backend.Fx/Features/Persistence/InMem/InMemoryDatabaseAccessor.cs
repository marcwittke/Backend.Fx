using System;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Util;

namespace Backend.Fx.Features.Persistence.InMem
{
    public interface IInMemoryDatabaseAccessor<TId> where TId : struct, IEquatable<TId>
    {
        IAggregateDictionaries<TId> GetAggregateDictionaries();
    }
    
    public class InMemoryDatabaseAccessor<TId> : IInMemoryDatabaseAccessor<TId> where TId : struct, IEquatable<TId>
    {
        private readonly InMemoryDatabase<TId> _inMemoryDatabase;

        public InMemoryDatabaseAccessor(InMemoryDatabase<TId> inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }
        
        public IAggregateDictionaries<TId> GetAggregateDictionaries()
        {
            return _inMemoryDatabase.GetInMemoryStores();
        }
    }
    
    public class MultiTenancyInMemoryDatabaseAccessor<TId> : IInMemoryDatabaseAccessor<TId> where TId : struct, IEquatable<TId>
    {
        private readonly InMemoryDatabase<TId> _inMemoryDatabase;
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;

        // ReSharper disable once UnusedParameter.Local
        public MultiTenancyInMemoryDatabaseAccessor(InMemoryDatabase<TId> inMemoryDatabase, ICurrentTHolder<TenantId> tenantIdHolder, IInMemoryDatabaseAccessor<TId> unused)
        {
            _inMemoryDatabase = inMemoryDatabase;
            _tenantIdHolder = tenantIdHolder;
        }
        
        public IAggregateDictionaries<TId> GetAggregateDictionaries()
        {
            return _inMemoryDatabase.GetInMemoryStoresOfTenant(_tenantIdHolder.Current);
        }
    }
}