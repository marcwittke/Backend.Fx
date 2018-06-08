namespace Backend.Fx.Patterns.EventAggregation.Domain
{
    /// <summary>
    /// Channel events from multiple objects into a single object to simplify registration for clients.
    /// https://martinfowler.com/eaaDev/EventAggregator.html
    /// </summary>
    public interface IDomainEventAggregator
    {
        void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent;
        void RaiseEvents();
    }
}