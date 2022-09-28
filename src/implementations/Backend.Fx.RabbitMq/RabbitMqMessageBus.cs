using System.Threading.Tasks;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Backend.Fx.RabbitMq
{
    public class RabbitMqMessageBus : MessageBus
    {
        private static readonly ILogger Logger = Log.Create<RabbitMqMessageBus>();
        private readonly RabbitMqChannel _channel;

        public RabbitMqMessageBus(
            IConnectionFactory connectionFactory,
            int retryCount,
            string exchangeName,
            string receiveQueueName)
        {
            _channel = new RabbitMqChannel(connectionFactory, exchangeName, receiveQueueName, retryCount);
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
        
        protected override void SubscribeToEventMessage(string eventTypeName)
        {
            Logger.LogInformation("Subscribing to messages of {EventTypeName}", eventTypeName);
            _channel.EnsureOpen();
            _channel.Subscribe(eventTypeName);
        }

        protected override Task PublishMessageAsync(SerializedMessage serializedMessage)
        {
            _channel.EnsureOpen();
            _channel.PublishMessage(serializedMessage);
            return Task.CompletedTask;
        }

        private async void ChannelOnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            Logger.LogDebug("RabbitMQ message with routing key {RoutingKey} received", args.RoutingKey);
            try
            {
                await ProcessAsync(new SerializedMessage(args.RoutingKey, args.Body));
                _channel.Acknowledge(args.DeliveryTag);
            }
            catch
            {
                _channel.NAcknowledge(args.DeliveryTag);
                throw;
            }
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
    }
}