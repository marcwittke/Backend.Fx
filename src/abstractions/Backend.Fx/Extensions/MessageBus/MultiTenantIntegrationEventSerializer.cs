using System.Globalization;
using System.Threading.Tasks;
using Backend.Fx.Exceptions;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Util;

namespace Backend.Fx.Extensions.MessageBus
{
    public class MultiTenantIntegrationEventSerializer : IIntegrationEventSerializer
    {
        private const string TenantIdPropertyKey = nameof(TenantId);
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;
        private readonly IIntegrationEventSerializer _serializer;

        public MultiTenantIntegrationEventSerializer(IIntegrationEventSerializer serializer, ICurrentTHolder<TenantId> tenantIdHolder)
        {
            _serializer = serializer;
            _tenantIdHolder = tenantIdHolder;
        }

        public Task<SerializedMessage> SerializeAsync<T>(T integrationEvent) where T : IIntegrationEvent
        {
            integrationEvent.Properties[TenantIdPropertyKey] =
                _tenantIdHolder.Current.Value.ToString(CultureInfo.InvariantCulture);
            return _serializer.SerializeAsync(integrationEvent);
        }

        public async Task<IIntegrationEvent> DeserializeAsync(SerializedMessage serializedMessage)
        {
            var integrationEvent = await _serializer.DeserializeAsync(serializedMessage).ConfigureAwait(false);
            
            if (!integrationEvent.Properties.TryGetValue(TenantIdPropertyKey, out var tenantIdString))
            {
                throw new UnprocessableException("Received an integration event message without TenantId property");
            }
            
            if (!int.TryParse(tenantIdString, out int tenantId))
            {
                throw new UnprocessableException(
                    $"Received an integration event message with an invalid TenantId property value: {tenantIdString}");
            }
            
            _tenantIdHolder.ReplaceCurrent(new TenantId(tenantId));
            
            return integrationEvent;
        }
    }
}