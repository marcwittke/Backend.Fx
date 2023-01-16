using System;
using System.Linq;
using Backend.Fx.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Persistence.InMem
{
    public class InMemoryPersistenceModule<TId> : PersistenceModule where TId : IEquatable<TId>
    {
        public override void Register(ICompositionRoot compositionRoot)
        {
            base.Register(compositionRoot);
            
            // a singleton database
            compositionRoot.Register(ServiceDescriptor.Singleton<InMemoryDatabase, InMemoryDatabase>());

            // a scoped accessor to get the aggregate dictionaries 
            compositionRoot.Register(ServiceDescriptor.Scoped<IInMemoryDatabaseAccessor,InMemoryDatabaseAccessor>());

            // we use the scoped accessor to get the aggregate dictionaries. This will be decorated in case
            // of Multi Tenancy to switch the tenant store (we provide it as generic and non generic version, too)
            compositionRoot.Register(ServiceDescriptor.Scoped(sp =>
                sp.GetRequiredService<IInMemoryDatabaseAccessor>().GetAggregateDictionaries()));
            compositionRoot.Register(ServiceDescriptor.Scoped<IAggregateDictionaries>(sp =>
                sp.GetRequiredService<IInMemoryDatabaseAccessor>().GetAggregateDictionaries()));

            compositionRoot.Register(
                ServiceDescriptor.Scoped(typeof(IAggregateQueryable<,>),typeof(InMemoryQueryable<,>)));
            
            compositionRoot.Register(
                ServiceDescriptor.Scoped(typeof(IRepository<,>), typeof(Repository<,>)));
        }

        public override IModule MultiTenancyModule => new InMemoryMultiTenancyPersistenceModule<TId>();
    }

    public class InMemoryMultiTenancyPersistenceModule<TId> : IModule where TId : IEquatable<TId>
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