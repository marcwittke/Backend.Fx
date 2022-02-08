namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using System.Threading.Tasks;
    using Environment.MultiTenancy;

    public class InMemoryMessageBus : MessageBus
    {
        private readonly InMemoryMessageBusChannel _channel;

        public InMemoryMessageBus()
        {
            _channel = new InMemoryMessageBusChannel();
        }
        
        public InMemoryMessageBus(InMemoryMessageBusChannel channel)
        {
            _channel = channel;
        }
        
        public override void Connect()
        {
            _channel.MessageReceived += ChannelOnMessageReceived;
        }

        protected override void Dispose(bool disposing)
        {
            _channel.MessageReceived -= ChannelOnMessageReceived;
        }

        protected override Task PublishOnMessageBus(IIntegrationEvent integrationEvent)
        {
            _channel.Publish(integrationEvent);
            
            // the returning Task is about publishing the event, not processing!
            return Task.CompletedTask;
        }

        protected override void Subscribe(string eventName)
        {
        }

        protected override void Unsubscribe(string eventName)
        {
        }

        private void ChannelOnMessageReceived(
            object sender, 
            InMemoryMessageBusChannel.MessageReceivedEventArgs eventArgs)
        {
            Process(
                MessageNameProvider.GetMessageName(eventArgs.IntegrationEvent), 
                new InMemoryProcessingContext(eventArgs.IntegrationEvent));
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