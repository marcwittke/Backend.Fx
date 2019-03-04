using System;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class DelegateIntegrationEventHandler<TIntegrationEvent> 
        : IIntegrationEventHandler<TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent
    {
        private readonly Action<TIntegrationEvent> _handleAction;

        public DelegateIntegrationEventHandler(Action<TIntegrationEvent> handleAction)
        {
            _handleAction = handleAction;
        }

        public void Handle(TIntegrationEvent eventData)
        {
            _handleAction.Invoke(eventData);
        }
    }
}
