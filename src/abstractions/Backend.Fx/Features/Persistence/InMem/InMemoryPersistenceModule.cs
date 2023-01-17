using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Domain;
using Backend.Fx.Features.Persistence;
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
                ServiceDescriptor.Scoped(typeof(IRepository<,>), typeof(QueryableRepository<,>)));
            
            compositionRoot.Register(
                ServiceDescriptor.Scoped<IUnitOfWork, FakeUnitOfWork>());
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

public class FakeUnitOfWork : IUnitOfWork
{
    public Task RegisterNewAsync(IAggregateRoot aggregateRoot, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }

    public Task RegisterDirtyAsync(IAggregateRoot aggregateRoot, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }

    public Task RegisterDeletedAsync(IAggregateRoot aggregateRoot, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }

    public Task RegisterDirtyAsync(IAggregateRoot[] aggregateRoots, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}