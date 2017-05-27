namespace Backend.Fx.Testing
{
    using System.Collections.Generic;
    using System.Linq;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;
    using RandomData;

    public class InMemoryRepository<T> : Repository<T> where T : AggregateRoot
    {
        public virtual Dictionary<int, T> Store { get; } = new Dictionary<int, T>();

        public InMemoryRepository(ICurrentTHolder<TenantId> tenantIdHolder, IAggregateRootAuthorization<T> aggregateRootAuthorization) : base(tenantIdHolder, aggregateRootAuthorization)
        {}

        protected override IQueryable<T> RawAggregateQueryable
        {
            get { return Store.Values.AsQueryable(); }
        }

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
            aggregateRoot.Id = Store.Keys.Any() ? Store.Keys.Max() + 1 : 1;
            Store.Add(aggregateRoot.Id, aggregateRoot);
        }

        protected override void DeletePersistent(T aggregateRoot)
        {
            Store.Remove(aggregateRoot.Id);
        }
    }
}