using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using System.Threading.Tasks;
    using Environment.MultiTenancy;

    public class InMemoryEventBus : EventBus
    {
        public InMemoryEventBus(IBackendFxApplication application)
                : base(application)
        {
        }

        public override void Connect()
        { }

        public override void Publish(IIntegrationEvent integrationEvent)
        {
#pragma warning disable 4014
            // Processing is done on the thread pool and not being awaited. This emulates best the behavior of a real
            // event bus, that incorporates network transfer and another system handling the event
            ProcessAsync(integrationEvent.GetType().FullName, new InMemoryProcessingContext(integrationEvent));
#pragma warning restore 4014
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
