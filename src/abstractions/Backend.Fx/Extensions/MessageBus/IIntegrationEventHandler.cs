using System.Threading.Tasks;

namespace Backend.Fx.Extensions.MessageBus
{
    internal interface IIntegrationEventHandler<T>
    {
        Task HandleAsync(T integrationEvent);
    }
}