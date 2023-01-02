using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.DomainEvents
{
    internal class DomainEventsModule : IModule
    {
        private readonly ILogger _logger = Log.Create<DomainEventsModule>();
        private readonly Assembly[] _assemblies;
        
        public DomainEventsModule(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }
        
        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(ServiceDescriptor.Scoped(sp => new DomainEventAggregator(sp)));
            compositionRoot.Register(ServiceDescriptor.Scoped<IDomainEventAggregator>(sp => sp.GetRequiredService<DomainEventAggregator>()));
            compositionRoot.Register(ServiceDescriptor.Scoped<IDomainEventAggregatorScope>(sp => sp.GetRequiredService<DomainEventAggregator>()));
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

                if (serviceDescriptors.Any())
                {
                    compositionRoot.RegisterCollection(serviceDescriptors);
                }
                else
                {
                    _logger.LogInformation("No handlers for {DomainEventType} found", domainEventType);
                }
            }
        }
    }
}