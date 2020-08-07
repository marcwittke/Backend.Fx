namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System;
    using Fx.Patterns.EventAggregation.Integration;

    public class ThrowingDynamicMessageHandler : IIntegrationMessageHandler
    {
        private readonly IIntegrationMessageHandler _handler;

        public ThrowingDynamicMessageHandler(IIntegrationMessageHandler handler)
        {
            _handler = handler;
        }

        public const string ExceptionMessage = "From ThrowingDynamicEventHandler";

        public void Handle(dynamic eventData)
        {
            _handler.Handle(eventData);
            throw new InvalidOperationException(ExceptionMessage);
        }
    }
}