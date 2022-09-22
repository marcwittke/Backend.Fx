using System;
using Backend.Fx.Extensions.MessageBus;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class ThrowingDynamicEventHandler : IIntegrationEventHandler<>
    {
        public const string ExceptionMessage = "From ThrowingDynamicEventHandler";
        private readonly IIntegrationEventHandler<> _handler;

        public ThrowingDynamicEventHandler(IIntegrationEventHandler<> handler)
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