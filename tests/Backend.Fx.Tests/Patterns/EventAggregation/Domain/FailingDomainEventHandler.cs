using System;
using Backend.Fx.Features.DomainEvents;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Domain
{
    public class FailingDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public void Handle(TestDomainEvent testDomainEvent)
        {
            throw new NotSupportedException();
        }
    }
}