using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend.Fx.Features.MessageBus
{
    public interface IIntegrationEventSerializer
    {
        Task<SerializedMessage> SerializeAsync<T>(T integrationEvent) where T : IIntegrationEvent;
        Task<IIntegrationEvent> DeserializeAsync(SerializedMessage serializedMessage);
    }
    
    public class IntegrationEventSerializer : IIntegrationEventSerializer
    {
        public async Task<SerializedMessage> SerializeAsync<T>(T integrationEvent)
            where T : IIntegrationEvent
        {
            using var memoryStream = new MemoryStream(4096);
            await JsonSerializer.SerializeAsync(memoryStream, integrationEvent).ConfigureAwait(false);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new SerializedMessage(typeof(T).Name, memoryStream.ToArray());
        }
        
        public async Task<IIntegrationEvent> DeserializeAsync(SerializedMessage serializedMessage)
        {
            using var memoryStream = new MemoryStream(serializedMessage.MessagePayload, false);
            var integrationEvent = (IIntegrationEvent)await JsonSerializer
                .DeserializeAsync(
                    memoryStream,
                    Type.GetType(serializedMessage.EventTypeName))
                .ConfigureAwait(false);

            return integrationEvent;
        }
    }
}