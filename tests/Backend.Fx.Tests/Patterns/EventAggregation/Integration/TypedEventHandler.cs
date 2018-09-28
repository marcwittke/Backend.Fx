namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using Fx.Patterns.EventAggregation.Integration;

    public class TypedEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        private readonly IIntegrationEventHandler<TestIntegrationEvent> _integrationEventHandlerImplementation;

        public TypedEventHandler(IIntegrationEventHandler<TestIntegrationEvent> integrationEventHandlerImplementation)
        {
            this._integrationEventHandlerImplementation = integrationEventHandlerImplementation;
        }

        public void Handle(TestIntegrationEvent eventData)
        {
            _integrationEventHandlerImplementation.Handle(eventData);
            eventData.Processed.Set();
        }
    }
}