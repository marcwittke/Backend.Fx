using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.RabbitMq
{
    public class RabbitMqChannel : IDisposable
    {
        private static readonly ILogger Logger = Log.Create<RabbitMqChannel>();
        private readonly string _exchangeName;
        private readonly IMessageNameProvider _messageNameProvider;
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _queueName;
        private readonly int _retryCount;
        private readonly HashSet<string> _subscribedEventNames = new HashSet<string>();
        private readonly object _syncRoot = new object();

        private IConnection _connection;
        private EventingBasicConsumer _consumer;
        private bool _isDisposed;
        private IModel _channel;

        public RabbitMqChannel(IMessageNameProvider messageNameProvider,
                               IConnectionFactory connectionFactory,
                               string exchangeName,
                               string queueName,
                               int retryCount)
        {
            _messageNameProvider = messageNameProvider;
            _connectionFactory = connectionFactory;
            _exchangeName = exchangeName;
            _queueName = queueName;
            _retryCount = retryCount;
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            try
            {
                EnsureClosed();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Closing RabbitMQ channel failed");
            }
        }

        public void EnsureClosed()
        {
            if (_consumer != null)
            {
                _consumer.Received -= OnMessageReceived;
                _consumer = null;
            }

            if (_channel != null)
            {
                _channel.CallbackException -= OnCallbackException;
                if (_channel.IsOpen) _channel.Close();

                _channel.Dispose();
                _channel = null;
            }

            if (_connection != null)
            {
                _connection.ConnectionShutdown -= OnConnectionShutdown;
                _connection.CallbackException -= OnCallbackException;
                _connection.ConnectionBlocked -= OnConnectionBlocked;
                if (_connection.IsOpen) _connection.Close();

                _connection.Dispose();
                _connection = null;
            }
        }

        public bool EnsureOpen()
        {
            if (!_isDisposed && _connection?.IsOpen == true && _channel?.IsOpen == true && _consumer?.IsRunning == true)
            {
                return true;
            }

            return Open();
        }

        public event EventHandler<BasicDeliverEventArgs> MessageReceived;

        private bool Open()
        {
            lock (_syncRoot)
            {
                EnsureClosed();

                Logger.LogInformation("RabbitMQ Client is trying to connect");
                Policy.Handle<SocketException>()
                      .Or<BrokerUnreachableException>()
                      .WaitAndRetry(_retryCount,
                          retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                          (ex, time) => { Logger.LogWarning(ex, "Connection not ready"); }
                      )
                      .Execute(() => { _connection = _connectionFactory.CreateConnection(); });

                if (_connection?.IsOpen == true)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    Logger.LogInformation("Acquired a connection to RabbitMQ host {HostName} and is subscribed to failure events",
                        _connection.Endpoint.HostName);

                    _channel = _connection.CreateModel();
                    _channel.ExchangeDeclare(_exchangeName, "direct");
                    _channel.QueueDeclare(_queueName, true, false, false, null);
                    _consumer = new EventingBasicConsumer(_channel);
                    _consumer.Received += OnMessageReceived;
                    _channel.BasicConsume(_queueName, false, _consumer);
                    _channel.CallbackException += OnCallbackException;

                    foreach (var subscribedEventName in _subscribedEventNames)
                    {
                        Logger.LogInformation(
                            "Binding messages on exchange {ExchangeName} with routing key {RoutingKey} to queue {QueueName}",
                            _exchangeName,
                            subscribedEventName,
                            _queueName);
                        _channel.QueueBind(_queueName, _exchangeName, subscribedEventName);
                    }

                    return true;
                }

                Logger.LogError("RabbitMQ connection could not be created and opened");
                return false;
            }
        }

        public void PublishEvent(IIntegrationEvent integrationEvent)
        {
            var messageName = _messageNameProvider.GetMessageName(integrationEvent);
            var message = JsonConvert.SerializeObject(integrationEvent);
            var body = Encoding.UTF8.GetBytes(message);

            DoResilient(() => _channel.BasicPublish(_exchangeName, messageName, null, body));
        }

        public void Subscribe(string messageName)
        {
            EnsureOpen();
            _channel.QueueBind(_queueName, _exchangeName, messageName);
            _subscribedEventNames.Add(messageName);
        }

        public void Unsubscribe(string eventName)
        {
            EnsureOpen();
            _channel.QueueUnbind(_queueName, _exchangeName, eventName);
            _subscribedEventNames.Remove(eventName);
        }

        public void Acknowledge(ulong deliveryTag)
        {
            Logger.LogDebug("Acknowledging {DeliveryTag}", deliveryTag);
            DoResilient(() => _channel.BasicAck(deliveryTag, false));
        }

        public void NAcknowledge(ulong deliveryTag)
        {
            Logger.LogDebug("NAcknowledging {DeliveryTag}", deliveryTag);
            DoResilient(() => _channel.BasicNack(deliveryTag, false, false));
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_isDisposed) return;

            Logger.LogWarning(e.Exception, "A RabbitMQ connection threw an exception");
            Open();
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_isDisposed) return;

            Logger.LogWarning("A RabbitMQ connection is blocked with reason {Reason}", e.Reason);
            Open();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_isDisposed) return;

            Logger.LogWarning("A RabbitMQ connection is shut down with reason {@Reason}", reason);
            Open();
        }

        private void OnMessageReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            MessageReceived?.Invoke(this, basicDeliverEventArgs);
        }

        private void DoResilient(Action action)
        {
            Policy.Handle<BrokerUnreachableException>()
                  .Or<SocketException>()
                  .WaitAndRetry(_retryCount,
                      retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                      (ex, time) => { Logger.LogWarning(ex, "Connection not ready"); })
                  .Execute(action);
        }
    }
}