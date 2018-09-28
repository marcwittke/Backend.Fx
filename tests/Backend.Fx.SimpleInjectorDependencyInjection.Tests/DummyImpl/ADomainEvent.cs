using Backend.Fx.Patterns.EventAggregation.Domain;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl
{
    public class ADomainEvent : IDomainEvent
    {
    }

    public class ADomainEventHandler1 : IDomainEventHandler<ADomainEvent>
    {
        private int _callCount;
        public void Handle(ADomainEvent domainEvent)
        {
            _callCount++;
        }
    }

    public class ADomainEventHandler2 : IDomainEventHandler<ADomainEvent>
    {
        private int _callCount;
        public void Handle(ADomainEvent domainEvent)
        {
            _callCount++;
        }
    }

    public class ADomainEventHandler3 : IDomainEventHandler<ADomainEvent>
    {
        private int _callCount;
        public void Handle(ADomainEvent domainEvent)
        {
            _callCount++;
        }
    }
}
