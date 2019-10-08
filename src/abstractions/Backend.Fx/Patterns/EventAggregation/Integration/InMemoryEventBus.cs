using System;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class InMemoryEventBus : EventBus
    {
        public InMemoryEventBus(IBackendFxApplication application)
                : base(application)
        { }

        public override void Connect()
        { }

        public override void Publish(IIntegrationEvent integrationEvent)
        {
            Process(integrationEvent.GetType().FullName, new InMemoryProcessingContext(integrationEvent));
        }

        protected override void Subscribe(string eventName)
        { }

        protected override void Unsubscribe(string eventName)
        { }

        private class InMemoryProcessingContext : EventProcessingContext
        {
            private readonly IIntegrationEvent _integrationEvent;

            public InMemoryProcessingContext(IIntegrationEvent integrationEvent)
            {
                _integrationEvent = integrationEvent;
            }

            public override TenantId TenantId => new TenantId(_integrationEvent.TenantId);

            public override dynamic DynamicEvent => _integrationEvent;

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return _integrationEvent;
            }
        }
    }
}
