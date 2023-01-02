using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.MessageBus;

namespace Backend.Fx.RabbitMq.Tests;

public class DummyIntegrationEventHandler : IIntegrationEventHandler<DummyIntegrationEvent>
{
    public static ManualResetEvent Called = new ManualResetEvent(false);

    public Task HandleAsync(DummyIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        Called.Set();
        return Task.CompletedTask;
    }
}