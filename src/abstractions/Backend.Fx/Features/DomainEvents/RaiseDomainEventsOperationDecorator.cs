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

        public Task BeginAsync(IServiceScope serviceScope)
        {
            return _operation.BeginAsync(serviceScope);
        }

        public async Task CompleteAsync()
        {
            await _domainEventAggregator.RaiseEventsAsync().ConfigureAwait(false);
            await _operation.CompleteAsync().ConfigureAwait(false);
        }

        public Task CancelAsync()
        {
            return _operation.CancelAsync();
        }
    }
}