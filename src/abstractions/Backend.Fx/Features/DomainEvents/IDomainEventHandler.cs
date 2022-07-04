namespace Backend.Fx.Features.DomainEvents
{
    public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
    {
        void Handle(TDomainEvent domainEvent);
    }
}