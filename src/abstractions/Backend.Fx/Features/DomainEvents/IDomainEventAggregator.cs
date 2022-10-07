using System.Threading.Tasks;

namespace Backend.Fx.Features.DomainEvents
{
    public interface IDomainEventAggregatorScope
    {
        void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent;
    }
    
    /// <summary>
    /// Channel events from multiple objects into a single object to simplify registration for clients.
    /// https://martinfowler.com/eaaDev/EventAggregator.html
    /// </summary>
    public interface IDomainEventAggregator
    {
        Task RaiseEventsAsync();
    }
}