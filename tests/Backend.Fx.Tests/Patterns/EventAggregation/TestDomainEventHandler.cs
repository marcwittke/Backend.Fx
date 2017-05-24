namespace Backend.Fx.Tests.Patterns.EventAggregation
{
    using System.Collections.Generic;
    using Fx.Patterns.EventAggregation;

    public class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public void Handle(TestDomainEvent testDomainEvent)
        {
            Events.Add(testDomainEvent);
        }

        public List<TestDomainEvent> Events { get; } = new List<TestDomainEvent>();
    }
}