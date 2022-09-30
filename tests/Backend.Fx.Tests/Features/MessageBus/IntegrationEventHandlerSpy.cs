using System.Threading.Tasks;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Tests.DummyServices;

namespace Backend.Fx.Tests.Features.MessageBus;

public interface IDummyIntegrationEventHandlerSpy
{
    public Task HandleAsync(DummyIntegrationEvent integrationEvent);
}

public class DummyIntegrationEventHandler : IIntegrationEventHandler<DummyIntegrationEvent>
{
    private readonly IDummyIntegrationEventHandlerSpy _spy;


    public DummyIntegrationEventHandler(IDummyIntegrationEventHandlerSpy spy)
    {
        _spy = spy;
    }


    public Task HandleAsync(DummyIntegrationEvent integrationEvent)
    {
        return _spy.HandleAsync(integrationEvent);
    }
}