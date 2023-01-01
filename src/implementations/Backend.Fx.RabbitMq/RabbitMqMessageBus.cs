using System.Threading.Tasks;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Backend.Fx.RabbitMq
{
    [PublicAPI]
    public class RabbitMqOptions
    {
        public string Hostname { get; set; }
        public int Port { get; set; } = 5672;
        public string Username { get; set; }
        public string Password { get; set; }
        public int RetryCount { get; set; } = 1;
        public string ExchangeName { get; set; }
        public string ReceiveQueueName  { get; set; }
    }
    
    public class RabbitMqMessageBus : MessageBus
    {
        private readonly ILogger _logger = Log.Create<RabbitMqMessageBus>();
        private readonly RabbitMqChannel _channel;

        public RabbitMqMessageBus(RabbitMqOptions options)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = options.Hostname,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password,
                UseBackgroundThreadsForIO = true
            };
            _channel = new RabbitMqChannel(connectionFactory, options.ExchangeName, options.ReceiveQueueName, options.RetryCount, ProcessAsync);
        }

        public override void Connect()
        {
            _logger.LogInformation("Opening a channel to RabbitMQ...");
            if (_channel.EnsureOpen())
            {
                _logger.LogInformation("Channel to RabbitMQ opened");
            }
        }
        
        protected override void SubscribeToEventMessage(string eventTypeName)
        {
            _logger.LogInformation("Subscribing to messages of {EventTypeName}", eventTypeName);
            _channel.EnsureOpen();
            _channel.Subscribe(eventTypeName);
        }

        protected override Task PublishMessageAsync(SerializedMessage serializedMessage)
        {
            _channel.EnsureOpen();
            _channel.PublishMessage(serializedMessage);
            return Task.CompletedTask;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (_channel != null)
                {
                    _logger.LogInformation("Closing RabbitMQ channel...");
                    _channel.Dispose();
                    _logger.LogInformation("RabbitMQ channel closed");
                }

            base.Dispose(disposing);
        }
    }
}