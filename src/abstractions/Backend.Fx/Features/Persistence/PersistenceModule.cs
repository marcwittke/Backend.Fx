using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Persistence
{
    public abstract class PersistenceModule : IModule
    {
        public virtual void Register(ICompositionRoot compositionRoot)
        {
            // register a default flush scoped instance, so that it can be decorated later
            compositionRoot.Register(ServiceDescriptor.Scoped<ICanFlush, DefaultFlush>());
            
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, FlushOperationDecorator>());
            
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped(typeof(IRepository<,>), typeof(Repository<,>)));
            
            // make sure we flush pending changes before raising pending domain events 
            compositionRoot.RegisterDecorator(
                ServiceDescriptor.Scoped<IDomainEventAggregator, FlushDomainEventAggregatorDecorator>());
        }

        public virtual IModule MultiTenancyModule => null;
    }
}