using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.RabbitMq.Tests
{
    public class TheRabbitMqChannel
    {
        private readonly ConnectionFactory _factory = new()
        {
            HostName = "localhost",
            UserName = "anicors",
            Password = "R4bb!tMQ"
        };

        private readonly AutoResetEvent _shutdown = new(false);
        private readonly ITestOutputHelper _testOutputHelper;

        public TheRabbitMqChannel(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        //[Fact]
        public void CanSendAndReceive()
        {
            var receivingChannel = new ReceivingChannel(_testOutputHelper, _factory);
            receivingChannel.Connect();

            var sendingChannel = new SendingChannel(_factory);
            sendingChannel.Connect();

            sendingChannel.Send("gnarf", "whatever");
            Assert.False(receivingChannel.Received.WaitOne(1000));

            sendingChannel.Send("info", "whatever");
            Assert.True(receivingChannel.Received.WaitOne(1000));

            _shutdown.Set();
        }


        private class SendingChannel
        {
            private readonly ConnectionFactory _connectionFactory;
            private IModel _channel;
            private IConnection _connection;

            public SendingChannel(ConnectionFactory connectionFactory)
            {
                _connectionFactory = connectionFactory;
            }

            public void Connect()
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare("direct_logs", "direct");
            }

            public void Send(string severity, string message)
            {
                byte[] body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish("direct_logs", severity, null, body);
            }
        }


        private class ReceivingChannel
        {
            private readonly ConnectionFactory _connectionFactory;
            private readonly ITestOutputHelper _testOutputHelper;
            private IModel _channel;
            private IConnection _connection;

            public ReceivingChannel(ITestOutputHelper testOutputHelper, ConnectionFactory connectionFactory)
            {
                _testOutputHelper = testOutputHelper;
                _connectionFactory = connectionFactory;
            }

            public AutoResetEvent Received { get; } = new(false);

            public void Connect()
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare("direct_logs", "direct");
                _channel.QueueDeclare("myqueue", true, false, false, null);
                _channel.QueueBind("myqueue", "direct_logs", "info");
                _testOutputHelper.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (_, ea) =>
                                     {
                                         byte[] body = ea.Body.ToArray();
                                         string message = Encoding.UTF8.GetString(body);
                                         string routingKey = ea.RoutingKey;
                                         _testOutputHelper.WriteLine($" [x] Received '{routingKey}':'{message}'");
                                         Received.Set();
                                         _channel.BasicAck(ea.DeliveryTag, false);
                                     };
                _channel.BasicConsume("myqueue", false, consumer);
            }
        }
    }
}
