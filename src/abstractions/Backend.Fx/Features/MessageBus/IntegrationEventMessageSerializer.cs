using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Backend.Fx.Features.MessageBus
{
    public interface IIntegrationEventMessageSerializer
    {
        Task<SerializedMessage> SerializeAsync(IIntegrationEvent integrationEvent);
        Task<IIntegrationEvent> DeserializeAsync(SerializedMessage serializedMessage);
    }

    public class IntegrationEventMessageMessageSerializer : IIntegrationEventMessageSerializer
    {
        private readonly IDictionary<string, Type> _typeMap;

        private static readonly JsonSerializerOptions SerializerOptions
            = new JsonSerializerOptions
                {
#if DEBUG
                    WriteIndented = true
#endif
                }
                .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        public IntegrationEventMessageMessageSerializer(IEnumerable<Type> eventTypesToSubscribe)
        {
            _typeMap = eventTypesToSubscribe.ToDictionary(
                evt => evt.Name,
                evt => evt);
        }

        public async Task<SerializedMessage> SerializeAsync(IIntegrationEvent integrationEvent)
        {
            using var memoryStream = new MemoryStream(4096);
            await JsonSerializer.SerializeAsync(memoryStream, integrationEvent, SerializerOptions)
                .ConfigureAwait(false);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new SerializedMessage(integrationEvent.GetType().Name, memoryStream.ToArray());
        }

        public async Task<IIntegrationEvent> DeserializeAsync(SerializedMessage serializedMessage)
        {
            using var memoryStream = new MemoryStream(serializedMessage.MessagePayload, false);
            Type returnType = _typeMap[serializedMessage.EventTypeName];
            var integrationEvent = (IIntegrationEvent)await JsonSerializer
                .DeserializeAsync(memoryStream, returnType, SerializerOptions)
                .ConfigureAwait(false);

            return integrationEvent;
        }
    }
}