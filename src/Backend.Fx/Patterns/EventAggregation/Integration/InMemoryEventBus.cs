namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using DependencyInjection;
    using Environment.MultiTenancy;
    using Logging;

    public class InMemoryEventBus : EventBus
    {
        public InMemoryEventBus(IScopeManager scopeManager, IExceptionLogger exceptionLogger)
                : base(scopeManager, exceptionLogger)
        { }

        public override void Connect()
        { }

        public override void Publish(IIntegrationEvent integrationEvent)
        {
            Process(integrationEvent.GetType().FullName, new InMemoryProcessingContext(integrationEvent));
        }

        protected override void Subscribe(string eventName)
        {}

        protected override void Unsubscribe(string eventName)
        {}

        private class InMemoryProcessingContext : EventProcessingContext 
        {
            private readonly IIntegrationEvent integrationEvent;

            public InMemoryProcessingContext(IIntegrationEvent integrationEvent)
            {
                this.integrationEvent = integrationEvent;
            }

            public override TenantId TenantId
            {
                get { return new TenantId(integrationEvent.TenantId); }
            }

            public override dynamic DynamicEvent
            {
                get { return integrationEvent; }
            }

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return integrationEvent;
            }
        }
    }
}
