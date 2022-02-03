using System.Linq;
using System.Text;
using System.Threading;
using Backend.Fx.Tests;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.RabbitMq.Tests
{
    public class TheRabbitMqChannel: TestWithLogging
    {
        private readonly ITestOutputHelper _testOutputHelper;
        
        private readonly AutoResetEvent _shutdown = new AutoResetEvent(false);

        private readonly ConnectionFactory _factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "anicors",
            Password = "R4bb!tMQ"
        };

        public TheRabbitMqChannel(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        //[Fact]
        public void CanSendAndReceive()
        {
            var receivingChannel = new ReceivingChannel(_testOutputHelper, _factory);
            receivingChannel.Connect();

            var sendingChannel = new SendingChannel(_testOutputHelper, _factory);
            sendingChannel.Connect();
            
            sendingChannel.Send("gnarf", "whatever");
            Assert.False(receivingChannel.Received.WaitOne(1000));
            
            sendingChannel.Send("info", "whatever");
            Assert.True(receivingChannel.Received.WaitOne(1000));
            
            _shutdown.Set();
        }

        private class SendingChannel
        {
            private readonly ITestOutputHelper _testOutputHelper;
            private readonly ConnectionFactory _connectionFactory;
            private IConnection _connection;
            private IModel _channel;

            public SendingChannel(ITestOutputHelper testOutputHelper, ConnectionFactory connectionFactory)
            {
                _testOutputHelper = testOutputHelper;
                _connectionFactory = connectionFactory;
            }
            
            public void Connect()
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange: "direct_logs", type: "direct");
            }

            public void Send(string severity, string message)
            {
                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: "direct_logs", routingKey: severity, basicProperties: null, body: body);
            }
        }
        
        private class ReceivingChannel
        {
            private readonly ITestOutputHelper _testOutputHelper;
            private readonly ConnectionFactory _connectionFactory;
            private IConnection _connection;
            private IModel _channel;
            public AutoResetEvent Received { get; } = new AutoResetEvent(false);
            
            public ReceivingChannel(ITestOutputHelper testOutputHelper, ConnectionFactory connectionFactory)
            {
                _testOutputHelper = testOutputHelper;
                _connectionFactory = connectionFactory;
            }

            public void Connect()
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange: "direct_logs", type: "direct");
                _channel.QueueDeclare("myqueue", true, false, false, null);
                _channel.QueueBind(queue: "myqueue", exchange: "direct_logs", routingKey: "info");
                _testOutputHelper.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    _testOutputHelper.WriteLine($" [x] Received '{routingKey}':'{message}'");
                    Received.Set();
                    _channel.BasicAck(ea.DeliveryTag, false);
                };
                _channel.BasicConsume(queue: "myqueue", autoAck: false, consumer: consumer);
            }
        }
        
    }
}