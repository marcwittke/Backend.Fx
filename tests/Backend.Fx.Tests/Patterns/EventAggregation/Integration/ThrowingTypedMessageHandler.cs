namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System;
    using Fx.Patterns.EventAggregation.Integration;

    public class ThrowingTypedMessageHandler : IIntegrationMessageHandler<TestIntegrationEvent>
    {
        private readonly IIntegrationMessageHandler<TestIntegrationEvent> _handler;

        public ThrowingTypedMessageHandler(IIntegrationMessageHandler<TestIntegrationEvent> handler)
        {
            _handler = handler;
        }

        public const string ExceptionMessage = "From ThrowingTypedEventHandler";

        public void Handle(TestIntegrationEvent eventData)
        {
            _handler.Handle(eventData);
            throw new InvalidOperationException(ExceptionMessage);
        }
    }
}