using System;

namespace Backend.Fx.Patterns.EventAggregation.Domain
{
    public class DelegateDomainEventHandler<TEvent> : IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        private readonly Action<TEvent> _action;

        public DelegateDomainEventHandler(Action<TEvent> action)
        {
            this._action = action;
        }

        public void Handle(TEvent eventData)
        {
            _action(eventData);
        }
    }
}
