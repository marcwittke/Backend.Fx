namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System;
    using Fx.Patterns.EventAggregation.Integration;

    public class ThrowingTypedEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        public const string ExceptionMessage = "From ThrowingTypedEventHandler";

        public void Handle(TestIntegrationEvent eventData)
        {
            throw new NotSupportedException(ExceptionMessage);
        }
    }
}