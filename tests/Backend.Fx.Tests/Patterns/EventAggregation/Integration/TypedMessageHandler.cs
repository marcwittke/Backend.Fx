using Backend.Fx.Features.MessageBus;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class TypedMessageHandler : IIntegrationMessageHandler<TestIntegrationEvent>
    {
        private readonly IIntegrationMessageHandler<TestIntegrationEvent> _integrationMessageHandlerImplementation;

        public TypedMessageHandler(IIntegrationMessageHandler<TestIntegrationEvent> integrationMessageHandlerImplementation)
        {
            _integrationMessageHandlerImplementation = integrationMessageHandlerImplementation;
        }

        public void Handle(TestIntegrationEvent eventData)
        {
            _integrationMessageHandlerImplementation.Handle(eventData);
            eventData.TypedProcessed.Set();
        }
    }
}