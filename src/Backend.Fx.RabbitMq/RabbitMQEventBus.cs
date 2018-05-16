namespace Backend.Fx.RabbitMq
{
    using System;
    using System.Text;
    using Environment.MultiTenancy;
    using Logging;
    using Newtonsoft.Json.Linq;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation.Integration;
    using RabbitMQ.Client;

    public class RabbitMQEventBus : EventBus, IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<RabbitMQEventBus>();
        private readonly RabbitMQChannel channel;
        
        public RabbitMQEventBus(IScopeManager scopeManager,
                                IConnectionFactory connectionFactory,
                                int retryCount,
                                string brokerName,
                                string queueName)
                : base(scopeManager)
        {
            channel = new RabbitMQChannel(connectionFactory, brokerName, queueName, retryCount);
        }

        public void Connect()
        {
            if (channel.EnsureOpen())
            {
                channel.MessageReceived += async (sender, args) => {
                                               await Process(args.RoutingKey, args.Body);
                                           };
            }
        }

        public override void Publish(IntegrationEvent integrationEvent)
        {
            Logger.Info($"Publishing {integrationEvent.GetType().Name}");
            channel.EnsureOpen();
            channel.PublishEvent(integrationEvent);
        }

        protected override IntegrationEventData Deserialize(object rawEventPayload)
        {
            Logger.Debug($"Deserializing a message of type {rawEventPayload?.GetType().Name ?? "null"}");
            if (!(rawEventPayload is byte[] rawEventPayloadBytes))
            {
                throw new InvalidOperationException("Raw event payload is not a binary JSON string");
            }

            var jsonPayload = Encoding.UTF8.GetString(rawEventPayloadBytes);
            dynamic payload = JObject.Parse(jsonPayload);
            int tenantid = payload.tenantId;
            return new IntegrationEventData(new TenantId(tenantid), payload);
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

        public void Dispose()
        {
            channel?.Dispose();
        }
    }
}