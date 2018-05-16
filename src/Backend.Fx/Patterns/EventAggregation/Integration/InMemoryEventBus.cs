namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using DependencyInjection;
    using Environment.MultiTenancy;

    public class InMemoryEventBus : EventBus
    {
        public InMemoryEventBus(IScopeManager scopeManager) : base(scopeManager)
        { }

        public override void Publish(IntegrationEvent integrationEvent)
        {
            Process(integrationEvent.GetType().Name, integrationEvent).Start();
        }

        protected override void Subscribe(string eventName)
        {}

        protected override void Unsubscribe(string eventName)
        {}

        protected override IntegrationEventData Deserialize(object rawEventPayload)
        {
            IntegrationEvent integrationEvent = (IntegrationEvent) rawEventPayload;
            return new IntegrationEventData(new TenantId(integrationEvent.TenantId), integrationEvent);
        }
    }
}
