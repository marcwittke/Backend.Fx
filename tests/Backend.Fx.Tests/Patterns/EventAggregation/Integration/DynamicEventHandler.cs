namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using Fx.Patterns.EventAggregation.Integration;

    public class DynamicEventHandler : IIntegrationEventHandler
    {
        private readonly IIntegrationEventHandler _integrationEventHandlerImplementation;

        public DynamicEventHandler(IIntegrationEventHandler integrationEventHandlerImplementation)
        {
            this._integrationEventHandlerImplementation = integrationEventHandlerImplementation;
        }

        public void Handle(dynamic eventData)
        {
            _integrationEventHandlerImplementation.Handle(eventData);
        }
    }
}