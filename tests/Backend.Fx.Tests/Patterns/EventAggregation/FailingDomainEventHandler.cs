namespace Backend.Fx.Tests.Patterns.EventAggregation
{
    using System;
    using Fx.Patterns.EventAggregation.Domain;

    public class FailingDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public void Handle(TestDomainEvent testDomainEvent)
        {
            throw new NotSupportedException();
        }
    }
}