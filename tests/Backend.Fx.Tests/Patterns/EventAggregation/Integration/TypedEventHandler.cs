﻿using Backend.Fx.Extensions.MessageBus;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class TypedEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        private readonly IIntegrationEventHandler<TestIntegrationEvent> _integrationEventHandlerImplementation;

        public TypedEventHandler(IIntegrationEventHandler<TestIntegrationEvent> integrationEventHandlerImplementation)
        {
            _integrationEventHandlerImplementation = integrationEventHandlerImplementation;
        }

        public void Handle(TestIntegrationEvent eventData)
        {
            _integrationEventHandlerImplementation.Handle(eventData);
            eventData.TypedProcessed.Set();
        }
    }
}