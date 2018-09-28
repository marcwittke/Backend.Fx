namespace Backend.Fx.Patterns.EventAggregation.Domain
{
    using System.Collections.Generic;

    public interface IDomainEventHandlerProvider
    {
        /// <summary>
        /// get all domain event handlers that want to handle a specific domain event
        /// </summary>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <returns></returns>
        IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent;
    }
}