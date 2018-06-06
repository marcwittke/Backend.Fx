namespace Backend.Fx.Bootstrapping.Tests.DummyImpl
{
    using Patterns.EventAggregation.Domain;

    public class ADomainEvent : IDomainEvent
    {
    }

    public class ADomainEventHandler1 : IDomainEventHandler<ADomainEvent>
    {
        private int callCount;
        public void Handle(ADomainEvent domainEvent)
        {
            callCount++;
        }
    }

    public class ADomainEventHandler2 : IDomainEventHandler<ADomainEvent>
    {
        private int callCount;
        public void Handle(ADomainEvent domainEvent)
        {
            callCount++;
        }
    }

    public class ADomainEventHandler3 : IDomainEventHandler<ADomainEvent>
    {
        private int callCount;
        public void Handle(ADomainEvent domainEvent)
        {
            callCount++;
        }
    }
}
