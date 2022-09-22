using Backend.Fx.Features.DomainEvents;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Extensions.Persistence
{
    public class FlushDomainEventAggregatorDecorator : IDomainEventAggregator
    {
        private static readonly ILogger Logger = Log.Create<FlushDomainEventAggregatorDecorator>();
        
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
            Logger.LogDebug("Flushing before raising domain events");
            _canFlush.Flush();
            _domainEventAggregatorImplementation.RaiseEvents();
        }
    }
}