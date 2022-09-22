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
        private static readonly ILogger Logger = Log.Create<DomainEventsModule>();
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
            foreach (var domainEventType in _assemblies.GetImplementingTypes(typeof(IDomainEvent)))
            {
                var handlerTypeForThisDomainEventType = typeof(IDomainEventHandler<>).MakeGenericType(domainEventType);

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
                    Logger.LogInformation("No handlers for {DomainEventType} found", domainEventType);
                }
            }
        }
    }
}