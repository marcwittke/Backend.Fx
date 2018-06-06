namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System;
    using System.Threading.Tasks;
    using Fx.Environment.MultiTenancy;
    using Fx.Logging;
    using Fx.Patterns.DependencyInjection;
    using Fx.Patterns.EventAggregation.Integration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class SerializingEventBus : EventBus 
    {
        public SerializingEventBus(IScopeManager scopeManager, IExceptionLogger exceptionLogger) : base(scopeManager, exceptionLogger)
        { }

        public override void Connect()
        { }

        public override Task Publish(IIntegrationEvent integrationEvent)
        {
            var jsonString = JsonConvert.SerializeObject(integrationEvent);
            return Task.Run(() => Process(integrationEvent.GetType().FullName, new SerializingProcessingContext(jsonString)));
        }

        protected override void Subscribe(string eventName)
        {}

        protected override void Unsubscribe(string eventName)
        {}

        private class SerializingProcessingContext : EventProcessingContext 
        {
            private readonly string jsonString;
            
            public SerializingProcessingContext(string jsonString)
            {
                this.jsonString = jsonString;
                var eventStub = JsonConvert.DeserializeAnonymousType(jsonString, new {tenantId = 0});
                TenantId = new TenantId(eventStub.tenantId);
            }

            public override TenantId TenantId {get; }
            
            public override dynamic DynamicEvent
            {
                get { return JObject.Parse(jsonString); }
            }

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return (IIntegrationEvent) JsonConvert.DeserializeObject(jsonString, eventType);
            }
        }
    }
}