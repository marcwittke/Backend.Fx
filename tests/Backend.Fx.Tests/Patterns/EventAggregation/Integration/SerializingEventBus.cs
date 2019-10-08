using System;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class SerializingEventBus : EventBus 
    {
        public SerializingEventBus(IBackendFxApplication application) 
            : base(application)
        { }

        public override void Connect()
        { }

        public override void Publish(IIntegrationEvent integrationEvent)
        {
            var jsonString = JsonConvert.SerializeObject(integrationEvent);
            Process(integrationEvent.GetType().FullName, new SerializingProcessingContext(jsonString));
        }

        protected override void Subscribe(string eventName)
        {}

        protected override void Unsubscribe(string eventName)
        {}

        private class SerializingProcessingContext : EventProcessingContext 
        {
            private readonly string _jsonString;
            
            public SerializingProcessingContext(string jsonString)
            {
                _jsonString = jsonString;
                var eventStub = JsonConvert.DeserializeAnonymousType(jsonString, new {tenantId = 0});
                TenantId = new TenantId(eventStub.tenantId);
            }

            public override TenantId TenantId {get; }
            
            public override dynamic DynamicEvent => JObject.Parse(_jsonString);

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return (IIntegrationEvent) JsonConvert.DeserializeObject(_jsonString, eventType);
            }
        }
    }
}