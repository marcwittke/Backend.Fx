namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using Fx.Patterns.EventAggregation.Integration;

    public class TypedEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        private readonly IIntegrationEventHandler<TestIntegrationEvent> integrationEventHandlerImplementation;

        public TypedEventHandler(IIntegrationEventHandler<TestIntegrationEvent> integrationEventHandlerImplementation)
        {
            this.integrationEventHandlerImplementation = integrationEventHandlerImplementation;
        }

        public void Handle(TestIntegrationEvent eventData)
        {
            integrationEventHandlerImplementation.Handle(eventData);
        }
    }
}