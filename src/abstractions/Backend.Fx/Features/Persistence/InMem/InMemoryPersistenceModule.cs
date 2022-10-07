using System;
using System.Linq;
using Backend.Fx.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Persistence.InMem
{
    public class InMemoryPersistenceModule<TId> : PersistenceModule where TId : struct, IEquatable<TId>
    {
        protected override void RegisterInfrastructure(ICompositionRoot compositionRoot)
        {
            // a singleton database
            compositionRoot.Register(ServiceDescriptor.Singleton<InMemoryDatabase<TId>, InMemoryDatabase<TId>>());

            // a scoped accessor to get the aggregate dictionaries 
            compositionRoot.Register(ServiceDescriptor.Scoped<IInMemoryDatabaseAccessor<TId>, InMemoryDatabaseAccessor<TId>>());

            // we use the scoped accessor to get the aggregate dictionaries. This will be decorated in case
            // of Multi Tenancy to switch the tenant store (we provide it as generic and non generic version, too)
            compositionRoot.Register(ServiceDescriptor.Scoped(sp =>
                sp.GetRequiredService<IInMemoryDatabaseAccessor<TId>>().GetAggregateDictionaries()));
            compositionRoot.Register(ServiceDescriptor.Scoped<IAggregateDictionaries>(sp =>
                sp.GetRequiredService<IInMemoryDatabaseAccessor<TId>>().GetAggregateDictionaries()));

            compositionRoot.Register(
                ServiceDescriptor.Scoped(typeof(IQueryable<>),typeof(InMemoryQueryable<>)));
        }

        protected override void RegisterImplementationSpecificServices(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(
                ServiceDescriptor.Scoped(typeof(IRepository<,>), typeof(InMemoryRepository<,>)));
        }

        public override IModule MultiTenancyModule => new InMemoryMultiTenancyPersistenceModule<TId>();
    }

    public class InMemoryMultiTenancyPersistenceModule<TId> : IModule where TId : struct, IEquatable<TId>
    {
        public void Register(ICompositionRoot compositionRoot)
        {
            // the MultiTenancyInMemoryDatabaseAccessor used the ICurrentTHolder<TenantId> to determine the respective
            // in memory collection specific to this tenant id
            compositionRoot.RegisterDecorator(ServiceDescriptor
                .Scoped<IInMemoryDatabaseAccessor<TId>, MultiTenancyInMemoryDatabaseAccessor<TId>>());
        }
    }
}