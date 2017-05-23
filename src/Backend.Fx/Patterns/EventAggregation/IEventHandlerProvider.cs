namespace Backend.Fx.Patterns.EventAggregation
{
    using System.Collections.Generic;

    public interface IEventHandlerProvider
    {
        IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent;
    }
}