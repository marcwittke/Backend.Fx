using JetBrains.Annotations;

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
}