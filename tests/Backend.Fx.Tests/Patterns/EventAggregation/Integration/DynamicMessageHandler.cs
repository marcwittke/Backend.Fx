using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class DynamicMessageHandler : IIntegrationMessageHandler
    {
        private readonly IIntegrationMessageHandler _integrationMessageHandlerImplementation;

        public DynamicMessageHandler(IIntegrationMessageHandler integrationMessageHandlerImplementation)
        {
            _integrationMessageHandlerImplementation = integrationMessageHandlerImplementation;
        }

        public void Handle(dynamic eventData)
        {
            _integrationMessageHandlerImplementation.Handle(eventData);
        }
    }
}