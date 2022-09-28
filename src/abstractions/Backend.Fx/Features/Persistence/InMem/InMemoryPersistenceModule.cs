using Backend.Fx.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Persistence.InMem
{
    public class InMemoryPersistenceModule : PersistenceModule
    {
        protected override void RegisterInfrastructure(ICompositionRoot compositionRoot)
        {
            // a singleton database
            compositionRoot.Register(ServiceDescriptor.Singleton<InMemoryDatabase, InMemoryDatabase>());

            // a scoped accessor to get the aggregate dictionaries 
            compositionRoot.Register(ServiceDescriptor.Scoped<IInMemoryDatabaseAccessor, InMemoryDatabaseAccessor>());

            // we use the scoped accessor to get the aggregate dictionaries. This will be decorated in case of Multi Tenancy to switch the tenant store
            compositionRoot.Register(ServiceDescriptor.Scoped(sp =>
                sp.GetRequiredService<IInMemoryDatabaseAccessor>().GetAggregateDictionaries()));

            compositionRoot.Register(
                ServiceDescriptor.Scoped(typeof(IAggregateQueryable<,>), typeof(InMemoryAggregateQueryable<,>)));
        }

        protected override void RegisterImplementationSpecificServices(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(
                ServiceDescriptor.Scoped(typeof(IRepository<,>), typeof(InMemoryRepository<,>)));
        }

        public override IModule MultiTenancyModule => new InMemoryMultiTenancyPersistenceModule();
    }

    public class InMemoryMultiTenancyPersistenceModule : IModule
    {
        public void Register(ICompositionRoot compositionRoot)
        {
            // the MultiTenancyInMemoryDatabaseAccessor used the ICurrentTHolder<TenantId> to determine the respective
            // in memory collection specific to this tenant id
            compositionRoot.RegisterDecorator(ServiceDescriptor
                .Scoped<IInMemoryDatabaseAccessor, MultiTenancyInMemoryDatabaseAccessor>());
        }
    }
}