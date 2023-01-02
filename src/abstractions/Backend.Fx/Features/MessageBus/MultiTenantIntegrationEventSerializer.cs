using System.Globalization;
using System.Threading.Tasks;
using Backend.Fx.Exceptions;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Util;

namespace Backend.Fx.Features.MessageBus
{
    public class MultiTenancyIntegrationEventMessageSerializer : IIntegrationEventMessageSerializer
    {
        private const string TenantIdPropertyKey = nameof(TenantId);
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;
        private readonly IIntegrationEventMessageSerializer _messageSerializer;

        public MultiTenancyIntegrationEventMessageSerializer(IIntegrationEventMessageSerializer messageSerializer, ICurrentTHolder<TenantId> tenantIdHolder)
        {
            _messageSerializer = messageSerializer;
            _tenantIdHolder = tenantIdHolder;
        }

        public Task<SerializedMessage> SerializeAsync(IIntegrationEvent integrationEvent)
        {
            integrationEvent.Properties[TenantIdPropertyKey] =
                _tenantIdHolder.Current.Value.ToString(CultureInfo.InvariantCulture);
            return _messageSerializer.SerializeAsync(integrationEvent);
        }

        public async Task<IIntegrationEvent> DeserializeAsync(SerializedMessage serializedMessage)
        {
            IIntegrationEvent integrationEvent = await _messageSerializer.DeserializeAsync(serializedMessage).ConfigureAwait(false);
            
            if (!integrationEvent.Properties.TryGetValue(TenantIdPropertyKey, out var tenantIdString))
            {
                throw new UnprocessableException("Received an integration event message without TenantId property");
            }
            
            if (!int.TryParse(tenantIdString, out var tenantId))
            {
                throw new UnprocessableException(
                    $"Received an integration event message with an invalid TenantId property value: {tenantIdString}");
            }
            
            _tenantIdHolder.ReplaceCurrent(new TenantId(tenantId));
            
            return integrationEvent;
        }
    }
}