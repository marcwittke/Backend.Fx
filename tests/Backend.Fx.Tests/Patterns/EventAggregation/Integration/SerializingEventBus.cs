using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System;
    using System.Threading.Tasks;
    using Fx.Environment.MultiTenancy;
    using Fx.Patterns.EventAggregation.Integration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class SerializingEventBus : EventBus 
    {
        public SerializingEventBus(IBackendFxApplication application) 
            : base(application)
        { }

        public override void Connect()
        { }

        protected override Task PublishOnEventBus(IIntegrationEvent integrationEvent)
        {
            var jsonString = JsonConvert.SerializeObject(integrationEvent);
            return Task.Run(()=>Process(integrationEvent.GetType().FullName, new SerializingProcessingContext(jsonString)));
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
                var eventStub = JsonConvert.DeserializeAnonymousType(jsonString, new {tenantId = 0, correlationId = Guid.Empty});
                TenantId = new TenantId(eventStub.tenantId);
                CorrelationId = eventStub.correlationId;
            }

            public override TenantId TenantId {get; }
            
            public override dynamic DynamicEvent => JObject.Parse(_jsonString);
            public override Guid CorrelationId { get; }

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return (IIntegrationEvent) JsonConvert.DeserializeObject(_jsonString, eventType);
            }
        }
    }
}