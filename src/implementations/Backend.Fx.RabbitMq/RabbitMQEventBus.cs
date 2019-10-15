using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.RabbitMq
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Environment.MultiTenancy;
    using Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Patterns.EventAggregation.Integration;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class RabbitMqEventBus : EventBus
    {
        private static readonly ILogger Logger = LogManager.Create<RabbitMqEventBus>();
        private readonly RabbitMqChannel _channel;
        
        public RabbitMqEventBus(IBackendFxApplication application,
                                IConnectionFactory connectionFactory,
                                int retryCount,
                                string brokerName,
                                string queueName)
                : base(application)
        {
            _channel = new RabbitMqChannel(connectionFactory, brokerName, queueName, retryCount);
        }

        public override void Connect()
        {
            Logger.Info("Opening a channel to RabbitMQ...");
            if (_channel.EnsureOpen())
            {
                _channel.MessageReceived += ChannelOnMessageReceived;
                Logger.Info("Channel to RabbitMQ opened");
            }
        }
        
        private void ChannelOnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            Logger.Debug($"RabbitMQ message with routing key {args.RoutingKey} received");
            Process(args.RoutingKey, new RabbitMqEventProcessingContext(args.Body));
        }

        public override Task Publish(IIntegrationEvent integrationEvent)
        {
            Logger.Info($"Publishing {integrationEvent.GetType().Name}");
            _channel.EnsureOpen();
            _channel.PublishEvent(integrationEvent);
            return Task.CompletedTask;
        }
        
        protected override void Subscribe(string eventName)
        {
            Logger.Info($"Subscribing to {eventName}");
            _channel.EnsureOpen();
            _channel.Subscribe(eventName);
        }

        protected override void Unsubscribe(string eventName)
        {
            Logger.Info($"Unsubscribing from {eventName}");
            _channel.Unsubscribe(eventName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_channel != null)
                {
                    Logger.Info("Closing RabbitMQ channel...");
                    _channel.MessageReceived -= ChannelOnMessageReceived;
                    _channel.Dispose();
                    Logger.Info("RabbitMQ channel closed");
                }
            }

            base.Dispose(disposing);
        }

        private class RabbitMqEventProcessingContext : EventProcessingContext
        {
            private readonly string _jsonString;

            public RabbitMqEventProcessingContext(object rawReceivedMessage)
            { 
                Logger.Debug($"Deserializing a message of type {rawReceivedMessage?.GetType().FullName ?? "???"}");
                if (!(rawReceivedMessage is byte[] rawEventPayloadBytes))
                {
                    throw new InvalidOperationException("Raw event payload is not a binary JSON string");
                }

                _jsonString = Encoding.UTF8.GetString(rawEventPayloadBytes);
                var eventStub = JsonConvert.DeserializeAnonymousType(_jsonString, new {tenantId = 0});
                TenantId = new TenantId(eventStub.tenantId);
            }

            public override TenantId TenantId { get; }

            public override dynamic DynamicEvent => JObject.Parse(_jsonString);

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return (IIntegrationEvent) JsonConvert.DeserializeObject(_jsonString, eventType);
            }
        }
    }   
}