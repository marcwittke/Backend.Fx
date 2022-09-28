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
        private static readonly ILogger Logger = Log.Create<MessageBusModule>();
        private readonly MessageBus _messageBus;
        private readonly IBackendFxApplication _application;
        private readonly List<Type> _eventTypesToSubscribe = new();

        public MessageBusModule(MessageBus messageBus, IBackendFxApplication application)
        {
            _messageBus = messageBus;
            _application = application;
            _messageBus.Invoker = new IntegrationEventHandlingInvoker(application.ExceptionLogger, application.Invoker);
            _messageBus.CompositionRoot = application.CompositionRoot;
            
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

            compositionRoot.Register(ServiceDescriptor.Scoped<IntegrationEventSerializer, IntegrationEventSerializer>());
            
            // make sure all integration events are raised after completing an operation, but before ending the scope
            compositionRoot.RegisterDecorator(
                ServiceDescriptor.Scoped<IOperation, RaiseIntegrationEventsWhenOperationCompleted>());

            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTHolder<Correlation>, CurrentCorrelationHolder>());

            RegisterIntegrationEventHandlers(compositionRoot);
        }

        public void SubscribeToAllEvents()
        {
            foreach (var eventType in _eventTypesToSubscribe)
            {
                _messageBus.Subscribe(eventType);
            }
        }

        private void RegisterIntegrationEventHandlers(ICompositionRoot compositionRoot)
        {
            foreach (var integrationEventType in _application.Assemblies.GetImplementingTypes(typeof(IIntegrationEvent)))
            {
                var handlerTypeForThisIntegrationEventType =
                    typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEventType);

                var serviceDescriptors = _application.Assemblies
                    .GetImplementingTypes(handlerTypeForThisIntegrationEventType)
                    .Select(t =>
                        new ServiceDescriptor(handlerTypeForThisIntegrationEventType, t, ServiceLifetime.Scoped))
                    .ToArray();

                if (serviceDescriptors.Any())
                {
                    compositionRoot.RegisterCollection(serviceDescriptors);
                }
                else
                {
                    Logger.LogInformation("No handlers for {IntegrationEventType} found", integrationEventType);
                }

                _eventTypesToSubscribe.Add(integrationEventType);
            }
        }
    }
    
    internal class MultiTenancyMessageBusModule : IModule
    {
        public void Register(ICompositionRoot compositionRoot)
        {
            // enrich the integration event with a TenantId property
            compositionRoot.RegisterDecorator(
                ServiceDescriptor.Scoped<IIntegrationEventSerializer, MultiTenantIntegrationEventSerializer>());
        }
    }
}