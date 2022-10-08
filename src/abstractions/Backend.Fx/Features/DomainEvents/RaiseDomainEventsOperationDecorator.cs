using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.DomainEvents
{
    [UsedImplicitly]
    public class RaiseDomainEventsOperationDecorator : IOperation
    {
        private readonly IDomainEventAggregator _domainEventAggregator;
        private readonly IOperation _operation;

        public RaiseDomainEventsOperationDecorator(
            IDomainEventAggregator domainEventAggregator,
            IOperation operation)
        {
            _domainEventAggregator = domainEventAggregator;
            _operation = operation;
        }

        public Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
        {
            return _operation.BeginAsync(serviceScope, cancellationToken);
        }

        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            await _domainEventAggregator.RaiseEventsAsync(cancellationToken).ConfigureAwait(false);
            await _operation.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task CancelAsync(CancellationToken cancellationToken = default)
        {
            return _operation.CancelAsync(cancellationToken);
        }
    }
}