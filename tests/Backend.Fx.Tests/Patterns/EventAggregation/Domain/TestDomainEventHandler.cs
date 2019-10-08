using System.Threading.Tasks;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Domain
{
    using System.Collections.Generic;
    using Fx.Patterns.EventAggregation.Domain;

    public class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public Task HandleAsync(TestDomainEvent testDomainEvent)
        {
            Events.Add(testDomainEvent);
            return Task.CompletedTask;
        }

        public List<TestDomainEvent> Events { get; } = new List<TestDomainEvent>();
    }
}