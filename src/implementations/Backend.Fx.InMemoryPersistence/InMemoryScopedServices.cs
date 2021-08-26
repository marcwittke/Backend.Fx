using System.Reflection;
using System.Security.Principal;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection.Pure;

namespace Backend.Fx.InMemoryPersistence
{
    public abstract class InMemoryScopedServices : ScopedServices
    {
        private readonly IInMemoryStores _inMemoryStores;

        protected InMemoryScopedServices(IClock clock, IIdentity identity, TenantId tenantId, Assembly[] assemblies, IInMemoryStores inMemoryStores)
            : base(clock, identity, tenantId, assemblies)
        {
            _inMemoryStores = inMemoryStores;
        }

        protected override ICanFlush CreateCanFlush()
        {
            return new InMemoryFlush();
        }

        public override IRepository<TAggregateRoot> GetRepository<TAggregateRoot>()
        {
            return new InMemoryRepository<TAggregateRoot>(
                _inMemoryStores.GetStore<TAggregateRoot>(),
                TenantIdHolder,
                (IAggregateAuthorization<TAggregateRoot>)GetAggregateAuthorization(IdentityHolder, typeof(TAggregateRoot)));
        }
    }
}