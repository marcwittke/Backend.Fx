namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using System.Threading.Tasks;
    using Environment.MultiTenancy;

    public class InMemoryMessageBus : MessageBus
    {
        public override void Connect()
        {
        }

        protected override Task PublishOnMessageBus(IIntegrationEvent integrationEvent)
        {
            Task.Run(() => Process(MessageNameProvider.GetMessageName(integrationEvent), new InMemoryProcessingContext(integrationEvent)));

            // the returning Task is about publishing the event, not processing!
            return Task.CompletedTask;
        }

        protected override void Subscribe(string messageName)
        {
        }

        protected override void Unsubscribe(string messageName)
        {
        }

        private class InMemoryProcessingContext : EventProcessingContext
        {
            private readonly IIntegrationEvent _integrationEvent;

            public InMemoryProcessingContext(IIntegrationEvent integrationEvent)
            {
                _integrationEvent = integrationEvent;
            }

            public override TenantId TenantId => new TenantId(_integrationEvent.TenantId);

            public override dynamic DynamicEvent => _integrationEvent;
            public override Guid CorrelationId => _integrationEvent.CorrelationId;

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return _integrationEvent;
            }
        }
    }
}