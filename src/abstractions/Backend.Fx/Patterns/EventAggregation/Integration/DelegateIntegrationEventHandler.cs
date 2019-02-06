using System;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class DelegateIntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent> where TEvent : IIntegrationEvent
    {
        private readonly Action<TEvent> _action;

        public DelegateIntegrationEventHandler(Action<TEvent> action)
        {
            this._action = action;
        }

        public void Handle(TEvent eventData)
        {
            _action(eventData);
        }
    }

    public class DelegateIntegrationEventHandler : IIntegrationEventHandler
    {
        private readonly Action<dynamic> _action;

        public DelegateIntegrationEventHandler(Action<dynamic> action)
        {
            _action = action;
        }

        public void Handle(dynamic eventData)
        {
            _action(eventData);
        }
    }
}
