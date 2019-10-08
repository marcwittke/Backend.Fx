using System.Threading.Tasks;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Domain
{
    using System;
    using Fx.Patterns.EventAggregation.Domain;

    public class FailingDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public Task HandleAsync(TestDomainEvent testDomainEvent)
        {
            throw new NotSupportedException();
        }
    }
}