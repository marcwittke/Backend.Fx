using System;
using System.Collections.Generic;
using System.Reflection;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class MessageBusModule : IModule
    {
        private readonly IMessageBus _messageBus;
        private readonly IEnumerable<Assembly> _assemblies;

        public MessageBusModule(IMessageBus messageBus, IEnumerable<Assembly> assemblies)
        {
            _messageBus = messageBus;
            _assemblies = assemblies;
        }
        
        public void Register(ICompositionRoot compositionRoot)
        {
            // note tht there should be no reason to access the singleton message bus instance from the service provider

            // register the message bus scope
            compositionRoot.Register(
                new ServiceDescriptor(
                    typeof(IMessageBusScope),
                    sp => new MessageBusScope(
                        _messageBus,
                        sp.GetRequiredService<ICurrentTHolder<Correlation>>(),
                        sp.GetRequiredService<ICurrentTHolder<TenantId>>()),
                    ServiceLifetime.Scoped));

            // register typed handlers
            foreach (Type integrationEventType in _assemblies.GetImplementingTypes(typeof(IIntegrationEvent)))
            {
                Type handlerServiceType = typeof(IIntegrationMessageHandler<>).MakeGenericType(integrationEventType);
                foreach (var handlerImplementingType in _assemblies.GetImplementingTypes(handlerServiceType))
                {
                    compositionRoot.Register(
                        new ServiceDescriptor(
                            handlerServiceType,
                            handlerImplementingType,
                            ServiceLifetime.Scoped));
                }
            }
        }
    }
}