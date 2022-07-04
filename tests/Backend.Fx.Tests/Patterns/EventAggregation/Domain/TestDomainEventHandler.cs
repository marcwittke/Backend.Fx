using System.Collections.Generic;
using Backend.Fx.Features.DomainEvents;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Domain
{
    public class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public List<TestDomainEvent> Events { get; } = new List<TestDomainEvent>();

        public void Handle(TestDomainEvent testDomainEvent)
        {
            Events.Add(testDomainEvent);
        }
    }
}