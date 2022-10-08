using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.DomainEvents;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Persistence
{
    public class FlushDomainEventAggregatorDecorator : IDomainEventAggregator
    {
        private static readonly ILogger Logger = Log.Create<FlushDomainEventAggregatorDecorator>();
        
        private readonly ICanFlush _canFlush;
        private readonly IDomainEventAggregator _domainEventAggregator;

        public FlushDomainEventAggregatorDecorator(ICanFlush canFlush, IDomainEventAggregator domainEventAggregator)
        {
            _canFlush = canFlush;
            _domainEventAggregator = domainEventAggregator;
        }

        public async Task RaiseEventsAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogDebug("Flushing before raising domain events");
            _canFlush.Flush();
            await _domainEventAggregator.RaiseEventsAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}