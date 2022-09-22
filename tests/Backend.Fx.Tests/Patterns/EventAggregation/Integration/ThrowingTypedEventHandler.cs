using System;
using Backend.Fx.Extensions.MessageBus;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class ThrowingTypedEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        public const string ExceptionMessage = "From ThrowingTypedEventHandler";
        private readonly IIntegrationEventHandler<TestIntegrationEvent> _handler;

        public ThrowingTypedEventHandler(IIntegrationEventHandler<TestIntegrationEvent> handler)
        {
            _handler = handler;
        }

        public void Handle(TestIntegrationEvent eventData)
        {
            _handler.Handle(eventData);
            throw new InvalidOperationException(ExceptionMessage);
        }
    }
}