namespace Backend.Fx.RabbitMq
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Environment.MultiTenancy;
    using Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation.Integration;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class RabbitMqEventBus : EventBus
    {
        private static readonly ILogger Logger = LogManager.Create<RabbitMqEventBus>();
        private readonly RabbitMqChannel _channel;
        
        public RabbitMqEventBus(IScopeManager scopeManager,
                                IConnectionFactory connectionFactory,
                                IExceptionLogger exceptionLogger,
                                int retryCount,
                                string brokerName,
                                string queueName)
                : base(scopeManager, exceptionLogger)
        {
            _channel = new RabbitMqChannel(connectionFactory, brokerName, queueName, retryCount);
        }

        public override void Connect()
        {
            if (_channel.EnsureOpen())
            {
                _channel.MessageReceived += ChannelOnMessageReceived;
            }
        }
        
        private void ChannelOnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
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
                    _channel.MessageReceived -= ChannelOnMessageReceived;
                    _channel.Dispose();
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