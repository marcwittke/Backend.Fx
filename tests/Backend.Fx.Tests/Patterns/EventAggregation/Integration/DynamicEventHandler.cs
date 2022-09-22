using Backend.Fx.Extensions.MessageBus;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class DynamicEventHandler : IIntegrationEventHandler<>
    {
        private readonly IIntegrationEventHandler<> _integrationEventHandlerImplementation;

        public DynamicEventHandler(IIntegrationEventHandler<> integrationEventHandlerImplementation)
        {
            _integrationEventHandlerImplementation = integrationEventHandlerImplementation;
        }

        public void Handle(dynamic eventData)
        {
            _integrationEventHandlerImplementation.Handle(eventData);
        }
    }
}