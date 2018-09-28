namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System.Linq;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Extensions;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;
    using RandomData;

    public class InMemoryRepository<T> : Repository<T> where T : AggregateRoot
    {
        public virtual IInMemoryStore<T> Store { get; }

        public InMemoryRepository(IInMemoryStore<T> store, ICurrentTHolder<TenantId> currentTenantIdHolder, IAggregateAuthorization<T> aggregateAuthorization) 
            : base(currentTenantIdHolder, aggregateAuthorization)
        {
            Store = store;
        }

        protected override IQueryable<T> RawAggregateQueryable => Store.Values.AsQueryable();

        public void Clear()
        {
            Store.Clear();
        }

        public T Random()
        {
            return Store.Values.Random();
        }

        protected override void AddPersistent(T aggregateRoot)
        {
            Store.Add(aggregateRoot.Id, aggregateRoot);
        }

        protected override void AddRangePersistent(T[] aggregateRoots)
        {
            aggregateRoots.ForAll(AddPersistent);
        }

        protected override void DeletePersistent(T aggregateRoot)
        {
            Store.Remove(aggregateRoot.Id);
        }
    }
}