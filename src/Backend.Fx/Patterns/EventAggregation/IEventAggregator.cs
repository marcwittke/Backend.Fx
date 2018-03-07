namespace Backend.Fx.Patterns.EventAggregation
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Channel events from multiple objects into a single object to simplify registration for clients.
    /// https://martinfowler.com/eaaDev/EventAggregator.html
    /// </summary>
    public interface IEventAggregator
    {
        Task PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent;

        Task PublishIntegrationEvent<TIntegrationEvent>(TIntegrationEvent integrationEvent) where TIntegrationEvent : IIntegrationEvent;

        void SubscribeToIntegrationEvent<TIntegrationEvent>(Action<TIntegrationEvent> handler) where TIntegrationEvent : IIntegrationEvent;
    }
}