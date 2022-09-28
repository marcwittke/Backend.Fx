using System.Threading.Tasks;

namespace Backend.Fx.Features.MessageBus
{
    internal interface IIntegrationEventHandler<T>
    {
        Task HandleAsync(T integrationEvent);
    }
}