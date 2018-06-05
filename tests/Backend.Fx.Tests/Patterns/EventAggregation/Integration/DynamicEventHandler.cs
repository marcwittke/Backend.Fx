namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using Fx.Patterns.EventAggregation.Integration;

    public class DynamicEventHandler : IIntegrationEventHandler
    {
        private readonly IIntegrationEventHandler integrationEventHandlerImplementation;

        public DynamicEventHandler(IIntegrationEventHandler integrationEventHandlerImplementation)
        {
            this.integrationEventHandlerImplementation = integrationEventHandlerImplementation;
        }

        public void Handle(dynamic eventData)
        {
            integrationEventHandlerImplementation.Handle(eventData);
        }
    }
}