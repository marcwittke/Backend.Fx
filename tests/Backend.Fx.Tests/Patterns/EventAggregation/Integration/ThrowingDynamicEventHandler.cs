using System;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class ThrowingDynamicMessageHandler : IIntegrationMessageHandler
    {
        public const string ExceptionMessage = "From ThrowingDynamicEventHandler";
        private readonly IIntegrationMessageHandler _handler;

        public ThrowingDynamicMessageHandler(IIntegrationMessageHandler handler)
        {
            _handler = handler;
        }

        public void Handle(dynamic eventData)
        {
            _handler.Handle(eventData);
            throw new InvalidOperationException(ExceptionMessage);
        }
    }
}
