using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.EventAggregation.Domain;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public class EfFlushDomainEventAggregatorDecorator : IDomainEventAggregator
    {
        private readonly ICanFlush _canFlush;
        private readonly IDomainEventAggregator _domainEventAggregatorImplementation;

        public EfFlushDomainEventAggregatorDecorator(ICanFlush canFlush, IDomainEventAggregator domainEventAggregatorImplementation)
        {
            _canFlush = canFlush;
            _domainEventAggregatorImplementation = domainEventAggregatorImplementation;
        }

        public void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            _domainEventAggregatorImplementation.PublishDomainEvent(domainEvent);
        }

        public void RaiseEvents()
        {
            _canFlush.Flush();
            _domainEventAggregatorImplementation.RaiseEvents();
        }
    }
}