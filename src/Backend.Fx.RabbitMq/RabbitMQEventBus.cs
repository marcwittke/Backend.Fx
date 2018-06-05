namespace Backend.Fx.RabbitMq
{
    using System;
    using System.Text;
    using Environment.MultiTenancy;
    using Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation.Integration;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class RabbitMQEventBus : EventBus
    {
        private static readonly ILogger Logger = LogManager.Create<RabbitMQEventBus>();
        private readonly RabbitMQChannel channel;
        
        public RabbitMQEventBus(IScopeManager scopeManager,
                                IConnectionFactory connectionFactory,
                                IExceptionLogger exceptionLogger,
                                int retryCount,
                                string brokerName,
                                string queueName)
                : base(scopeManager, exceptionLogger)
        {
            channel = new RabbitMQChannel(connectionFactory, brokerName, queueName, retryCount);
        }

        public override void Connect()
        {
            if (channel.EnsureOpen())
            {
                channel.MessageReceived += ChannelOnMessageReceived;
            }
        }
        
        private void ChannelOnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            Process(args.RoutingKey, new RabbitMqEventProcessingContext(args.Body));
        }

        public override void Publish(IIntegrationEvent integrationEvent)
        {
            Logger.Info($"Publishing {integrationEvent.GetType().Name}");
            channel.EnsureOpen();
            channel.PublishEvent(integrationEvent);
        }
        
        protected override void Subscribe(string eventName)
        {
            Logger.Info($"Subscribing to {eventName}");
            channel.EnsureOpen();
            channel.Subscribe(eventName);
        }

        protected override void Unsubscribe(string eventName)
        {
            Logger.Info($"Unsubscribing from {eventName}");
            channel.Unsubscribe(eventName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (channel != null)
                {
                    channel.MessageReceived -= ChannelOnMessageReceived;
                    channel.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private class RabbitMqEventProcessingContext : EventProcessingContext
        {
            private readonly string jsonString;

            public RabbitMqEventProcessingContext(object rawReceivedMessage)
            { 
                Logger.Debug($"Deserializing a message of type {rawReceivedMessage?.GetType().FullName ?? "???"}");
                if (!(rawReceivedMessage is byte[] rawEventPayloadBytes))
                {
                    throw new InvalidOperationException("Raw event payload is not a binary JSON string");
                }

                jsonString = Encoding.UTF8.GetString(rawEventPayloadBytes);
                var eventStub = JsonConvert.DeserializeAnonymousType(jsonString, new {tenantId = 0});
                TenantId = new TenantId(eventStub.tenantId);
            }

            public override TenantId TenantId { get; }

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