using System.Threading.Tasks;

namespace Backend.Fx.Features.MessageBus
{
    public interface IIntegrationEventHandler<T>
    {
        Task HandleAsync(T integrationEvent);
    }
}