using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.Features.MessageBus
{
    [PublicAPI]
    public interface IIntegrationEventHandler<in T>
    {
        Task HandleAsync(T integrationEvent, CancellationToken cancellationToken = default);
    }
}