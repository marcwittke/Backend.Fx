namespace Backend.Fx.Tests.Patterns.EventAggregation
{
    using Fx.Patterns.EventAggregation.Domain;

    public class TestDomainEvent : IDomainEvent
    {
        public TestDomainEvent(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}