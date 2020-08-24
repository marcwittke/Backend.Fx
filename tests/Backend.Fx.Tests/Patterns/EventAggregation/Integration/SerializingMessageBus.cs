using System;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class SerializingMessageBus : MessageBus
    {
        public override void Connect()
        {
        }

        protected override Task PublishOnMessageBus(IIntegrationEvent integrationEvent)
        {
            var jsonString = JsonConvert.SerializeObject(integrationEvent);
            return Task.Run(() => Process(integrationEvent.GetType().FullName, new SerializingProcessingContext(jsonString)));
        }

        protected override void Subscribe(string messageName)
        {
        }

        protected override void Unsubscribe(string messageName)
        {
        }

        private class SerializingProcessingContext : EventProcessingContext
        {
            private readonly string _jsonString;

            public SerializingProcessingContext(string jsonString)
            {
                _jsonString = jsonString;
                var eventStub = JsonConvert.DeserializeAnonymousType(jsonString, new {tenantId = 0, correlationId = Guid.Empty});
                TenantId = new TenantId(eventStub.tenantId);
                CorrelationId = eventStub.correlationId;
            }

            public override TenantId TenantId { get; }

            public override dynamic DynamicEvent => JObject.Parse(_jsonString);
            public override Guid CorrelationId { get; }

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return (IIntegrationEvent) JsonConvert.DeserializeObject(_jsonString, eventType);
            }
        }
    }
}