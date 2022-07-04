using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.DomainEvents
{
    public class DomainEventsModule : IModule
    {
        private readonly Assembly[] _assemblies;
        
        public DomainEventsModule(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }
        
        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(ServiceDescriptor.Scoped<IDomainEventAggregator>(sp => new DomainEventAggregator(sp)));
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, RaiseDomainEventsOperationDecorator>());
            RegisterDomainEventHandlers(compositionRoot);
        }
        
        private void RegisterDomainEventHandlers(ICompositionRoot compositionRoot)
        {
            foreach (Type domainEventType in _assemblies.GetImplementingTypes(typeof(IDomainEvent)))
            {
                Type handlerTypeForThisDomainEventType = typeof(IDomainEventHandler<>).MakeGenericType(domainEventType);

                var serviceDescriptors = _assemblies
                    .GetImplementingTypes(handlerTypeForThisDomainEventType)
                    .Select(t => new ServiceDescriptor(handlerTypeForThisDomainEventType, t, ServiceLifetime.Scoped))
                    .ToArray();

                compositionRoot.RegisterCollection(serviceDescriptors);
            }
        }
    }
}