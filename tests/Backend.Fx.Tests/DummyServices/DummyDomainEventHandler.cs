using Backend.Fx.Features.DomainEvents;

namespace Backend.Fx.Tests.DummyServices;

public class DummyDomainEventHandler : IDomainEventHandler<DummyDomainEvent>
{
    private readonly IDummyDomainEventHandlerSpy _spy;

    public DummyDomainEventHandler(IDummyDomainEventHandlerSpy spy)
    {
        _spy = spy;
    }

    public void Handle(DummyDomainEvent domainEvent)
    {
        _spy.Handle(domainEvent);
    }
}

public interface IDummyDomainEventHandlerSpy
{
    void Handle(DummyDomainEvent dummyDomainEvent);
}