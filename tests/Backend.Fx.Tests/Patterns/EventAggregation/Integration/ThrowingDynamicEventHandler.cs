namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System;
    using Fx.Patterns.EventAggregation.Integration;

    public class ThrowingDynamicEventHandler : IIntegrationEventHandler
    {
        public const string ExceptionMessage = "From ThrowingDynamicEventHandler";

        public void Handle(dynamic eventData)
        {
            throw new InvalidOperationException(ExceptionMessage);
        }
    }
}