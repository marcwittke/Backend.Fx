using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.MessageBus
{
    internal class MessageBusModule : IModule
    {
        private readonly ILogger _logger = Log.Create<MessageBusModule>();
        private readonly IMessageBus _messageBus;
        private readonly IBackendFxApplication _application;
        private readonly List<Type> _eventTypesToSubscribe = new();

        public MessageBusModule(IMessageBus messageBus, IBackendFxApplication application)
        {
            _messageBus = messageBus;
            _application = application;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            // note tht there should be no reason to access the singleton message bus instance from the service provider

            // register the message bus scope
            compositionRoot.Register(
                ServiceDescriptor.Scoped<IMessageBusScope>(
                    sp => new MessageBusScope(
                        _messageBus,
                        sp.GetRequiredService<ICurrentTHolder<Correlation>>())));

            // make sure all integration events are raised after completing an operation, but before ending the scope
            compositionRoot.RegisterDecorator(
                ServiceDescriptor.Scoped<IOperation, RaiseIntegrationEventsWhenOperationCompleted>());

            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTHolder<Correlation>, CurrentCorrelationHolder>());

            RegisterIntegrationEventHandlers(compositionRoot);
            
            compositionRoot.Register(
                ServiceDescriptor.Singleton<IIntegrationEventMessageSerializer>(new IntegrationEventMessageMessageSerializer(_eventTypesToSubscribe)));
        }

        public void SubscribeToAllEvents()
        {
            foreach (Type eventType in _eventTypesToSubscribe)
            {
                _messageBus.Subscribe(eventType);
            }
        }

        private void RegisterIntegrationEventHandlers(ICompositionRoot compositionRoot)
        {
            foreach (Type integrationEventType in _application.Assemblies.GetImplementingTypes(typeof(IIntegrationEvent)))
            {
                Type handlerTypeForThisIntegrationEventType =
                    typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEventType);

                var serviceDescriptors = _application.Assemblies
                    .GetImplementingTypes(handlerTypeForThisIntegrationEventType)
                    .Select(t =>
                        new ServiceDescriptor(handlerTypeForThisIntegrationEventType, t, ServiceLifetime.Scoped))
                    .ToArray();

                if (serviceDescriptors.Any())
                {
                    _logger.LogInformation("Registering {Count} handlers for {IntegrationEventType}", 
                        serviceDescriptors.Length,
                        integrationEventType);
                    compositionRoot.RegisterCollection(serviceDescriptors);
                    _eventTypesToSubscribe.Add(integrationEventType);
                }
                else
                {
                    _logger.LogInformation("No handlers for {IntegrationEventType} found", integrationEventType);
                }
            }
        }
    }
    
    internal class MultiTenancyMessageBusModule : IModule
    {
        public void Register(ICompositionRoot compositionRoot)
        {
            // enrich the integration event with a TenantId property
            compositionRoot.RegisterDecorator(
                ServiceDescriptor.Scoped<IIntegrationEventMessageSerializer, MultiTenancyIntegrationEventMessageSerializer>());
        }
    }
}