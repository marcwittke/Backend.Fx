namespace Backend.Fx.Patterns.EventAggregation
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IEventHandlerProvider
    {
        /// <summary>
        /// get all deomain event handlers that want to handle a specific domain event
        /// </summary>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <returns></returns>
        IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent;
        IEnumerable<IAsyncDomainEventHandler<TDomainEvent>> GetAllAsyncEventHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent;
    }
}