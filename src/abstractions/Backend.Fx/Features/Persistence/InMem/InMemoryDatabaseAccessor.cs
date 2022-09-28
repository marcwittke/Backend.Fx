using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Util;

namespace Backend.Fx.Features.Persistence.InMem
{
    public interface IInMemoryDatabaseAccessor
    {
        IAggregateDictionaries GetAggregateDictionaries();
    }
    
    public class InMemoryDatabaseAccessor : IInMemoryDatabaseAccessor
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemoryDatabaseAccessor(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }
        
        public IAggregateDictionaries GetAggregateDictionaries()
        {
            return _inMemoryDatabase.GetInMemoryStores();
        }
    }
    
    public class MultiTenancyInMemoryDatabaseAccessor : IInMemoryDatabaseAccessor
    {
        private readonly InMemoryDatabase _inMemoryDatabase;
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;

        public MultiTenancyInMemoryDatabaseAccessor(InMemoryDatabase inMemoryDatabase, ICurrentTHolder<TenantId> tenantIdHolder, IInMemoryDatabaseAccessor inMemoryDatabaseAccessorUnused)
        {
            _inMemoryDatabase = inMemoryDatabase;
            _tenantIdHolder = tenantIdHolder;
        }
        
        public IAggregateDictionaries GetAggregateDictionaries()
        {
            return _inMemoryDatabase.GetInMemoryStoresOfTenant(_tenantIdHolder.Current);
        }
    }
}