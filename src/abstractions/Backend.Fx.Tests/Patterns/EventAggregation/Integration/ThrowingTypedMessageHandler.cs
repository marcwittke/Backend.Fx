using System;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class ThrowingTypedMessageHandler : IIntegrationMessageHandler<TestIntegrationEvent>
    {
        public const string ExceptionMessage = "From ThrowingTypedEventHandler";
        private readonly IIntegrationMessageHandler<TestIntegrationEvent> _handler;

        public ThrowingTypedMessageHandler(IIntegrationMessageHandler<TestIntegrationEvent> handler)
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
