using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Domain;

namespace Backend.Fx.Environment.Persistence
{
    public class FlushDomainEventAggregatorDecorator : IDomainEventAggregator
    {
        private static readonly ILogger Logger = LogManager.Create<FlushDomainEventAggregatorDecorator>();
        
        private readonly ICanFlush _canFlush;
        private readonly IDomainEventAggregator _domainEventAggregatorImplementation;

        public FlushDomainEventAggregatorDecorator(ICanFlush canFlush, IDomainEventAggregator domainEventAggregatorImplementation)
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
            Logger.Debug("Flushing before raising domain events");
            _canFlush.Flush();
            _domainEventAggregatorImplementation.RaiseEvents();
        }
    }
}