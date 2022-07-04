using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;

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

        public void Begin()
        {
            _operation.Begin();
        }

        public void Complete()
        {
            _domainEventAggregator.RaiseEvents();
            _operation.Complete();
        }

        public void Cancel()
        {
            _operation.Cancel();
        }
    }
}