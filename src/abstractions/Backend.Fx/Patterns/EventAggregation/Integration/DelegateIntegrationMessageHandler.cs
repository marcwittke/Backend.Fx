using System;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class DelegateIntegrationMessageHandler<TIntegrationEvent>
        : IIntegrationMessageHandler<TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent
    {
        private readonly Action<TIntegrationEvent> _handleAction;

        public DelegateIntegrationMessageHandler(Action<TIntegrationEvent> handleAction)
        {
            _handleAction = handleAction;
        }

        public void Handle(TIntegrationEvent eventData)
        {
            _handleAction.Invoke(eventData);
        }
    }
}