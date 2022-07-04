using System;
using System.Text;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.RabbitMq
{
    public class RabbitMqMessageBus : MessageBus
    {
        private static readonly ILogger Logger = Log.Create<RabbitMqMessageBus>();
        private readonly RabbitMqChannel _channel;

        public RabbitMqMessageBus(IConnectionFactory connectionFactory, int retryCount, string exchangeName, string receiveQueueName) 
        {
            _channel = new RabbitMqChannel(MessageNameProvider, connectionFactory, exchangeName, receiveQueueName, retryCount);
        }

        public override void Connect()
        {
            Logger.LogInformation("Opening a channel to RabbitMQ...");
            if (_channel.EnsureOpen())
            {
                _channel.MessageReceived += ChannelOnMessageReceived;
                Logger.LogInformation("Channel to RabbitMQ opened");
            }
        }

        private void ChannelOnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            Logger.LogDebug("RabbitMQ message with routing key {RoutingKey} received", args.RoutingKey);
            try
            {
                Process(args.RoutingKey, new RabbitMqEventProcessingContext(args.Body));
                _channel.Acknowledge(args.DeliveryTag);
            }
            catch
            {
                _channel.NAcknowledge(args.DeliveryTag);
                throw;
            }
        }

        protected override Task PublishOnMessageBus(IIntegrationEvent integrationEvent)
        {
            Logger.LogInformation("Publishing {MessageName}", MessageNameProvider.GetMessageName(integrationEvent));
            _channel.EnsureOpen();
            _channel.PublishEvent(integrationEvent);
            return Task.CompletedTask;
        }

        protected override void Subscribe(string messageName)
        {
            Logger.LogInformation("Subscribing to {MessageName}", messageName);
            _channel.EnsureOpen();
            _channel.Subscribe(messageName);
        }

        protected override void Unsubscribe(string messageName)
        {
            Logger.LogInformation("Unsubscribing from {MessageName}", messageName);
            _channel.Unsubscribe(messageName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (_channel != null)
                {
                    Logger.LogInformation("Closing RabbitMQ channel...");
                    _channel.MessageReceived -= ChannelOnMessageReceived;
                    _channel.Dispose();
                    Logger.LogInformation("RabbitMQ channel closed");
                }

            base.Dispose(disposing);
        }

        private class RabbitMqEventProcessingContext : EventProcessingContext
        {
            private readonly string _jsonString;

            public RabbitMqEventProcessingContext(object rawReceivedMessage)
            {
                Logger.LogTrace("Deserializing a message of type {MessageType}", rawReceivedMessage?.GetType().Name ?? "???");
                if (!(rawReceivedMessage is byte[] rawEventPayloadBytes)) throw new InvalidOperationException("Raw event payload is not a binary JSON string");

                _jsonString = Encoding.UTF8.GetString(rawEventPayloadBytes);
                var eventStub = JsonConvert.DeserializeAnonymousType(_jsonString, new {tenantId = 0, correlationId = Guid.Empty});
                TenantId = new TenantId(eventStub.tenantId);
                CorrelationId = eventStub.correlationId;
            }

            public override TenantId TenantId { get; }

            public override dynamic DynamicEvent => JObject.Parse(_jsonString);
            public override Guid CorrelationId { get; }

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return (IIntegrationEvent) JsonConvert.DeserializeObject(_jsonString, eventType);
            }
        }
    }
}