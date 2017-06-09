namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using BuildingBlocks;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Patterns.Authorization;
    using Patterns.UnitOfWork;
    using RandomData;

    public abstract class InMemoryPersistenceFixture
    {
        private readonly Dictionary<Type, object> stores;

        protected InMemoryPersistenceFixture(bool withDemoData, Assembly domainAssembly)
        {
            using (var dataGenerationRuntime = new DataGenerationRuntime(domainAssembly))
            {
                dataGenerationRuntime.Boot();
                TenantId = withDemoData
                               ? dataGenerationRuntime.TenantManager.CreateDemonstrationTenant("test", "", false)
                               : dataGenerationRuntime.TenantManager.CreateProductionTenant("test", "", false);
                dataGenerationRuntime.TenantManager.EnsureTenantIsInitialized(TenantId);
                stores = dataGenerationRuntime.Stores;
            }
        }

        public IClock Clock { get; } = new FrozenClock();

        public ICanFlush CanFlush { get; } = new DummyCanFlush();

        public TenantId TenantId { get; }

        public TAggregateRoot GetRandom<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            return LinqExtensions.Random<TAggregateRoot>(Repository<TAggregateRoot>().AggregateQueryable);
        }

        public InMemoryRepository<TAggregateRoot> Repository<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            return new InMemoryRepository<TAggregateRoot>((IInMemoryStore<TAggregateRoot>)stores[typeof(TAggregateRoot)], TenantId, new AllowAll<TAggregateRoot>());
        }

        public void ClearRepository<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            Repository<TAggregateRoot>().Clear();
        }

        private class DummyCanFlush : ICanFlush
        {
            public void Flush()
            { }
        }
    }
}