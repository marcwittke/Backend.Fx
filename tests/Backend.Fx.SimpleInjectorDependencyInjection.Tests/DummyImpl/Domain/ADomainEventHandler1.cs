using Backend.Fx.Patterns.EventAggregation.Domain;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain
{
    public class ADomainEventHandler1 : IDomainEventHandler<ADomainEvent>
    {
        public void Handle(ADomainEvent domainEvent)
        {
        }
    }
}