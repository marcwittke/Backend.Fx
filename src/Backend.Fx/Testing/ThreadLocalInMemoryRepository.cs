namespace Backend.Fx.Testing
{
    using System.Collections.Generic;
    using System.Threading;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;

    public class ThreadLocalInMemoryRepository<T> : InMemoryRepository<T> where T : AggregateRoot
    {
        private static readonly ThreadLocal<Dictionary<int, T>> ThreadLocalStore = new ThreadLocal<Dictionary<int, T>>(() => new Dictionary<int, T>());

        public override Dictionary<int, T> Store
        {
            get
            {
                return ThreadLocalStore.Value;
            }
        }

        public ThreadLocalInMemoryRepository(ICurrentTHolder<TenantId> tenantIdHolder, IAggregateRootAuthorization<T> aggregateRootAuthorization) : base(tenantIdHolder, aggregateRootAuthorization)
        { }
    }
}