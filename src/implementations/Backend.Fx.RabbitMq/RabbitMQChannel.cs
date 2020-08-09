using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Backend.Fx.RabbitMq
{
    public class RabbitMqChannel : IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<RabbitMqChannel>();
        private readonly string _brokerName;
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _queueName;
        private readonly int _retryCount;
        private readonly HashSet<string> _subscribedEventNames = new HashSet<string>();
        private readonly object _syncRoot = new object();

        private IConnection _connection;
        private EventingBasicConsumer _consumer;
        private bool _isDisposed;
        private IModel _model;

        public RabbitMqChannel(IConnectionFactory connectionFactory, string brokerName, string queueName, int retryCount)
        {
            _connectionFactory = connectionFactory;
            _brokerName = brokerName;
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
                Logger.Error(ex, "Closing RabbitMQ channel failed");
            }
        }

        public void EnsureClosed()
        {
            if (_consumer != null)
            {
                _consumer.Received -= OnMessageReceived;
                _consumer = null;
            }

            if (_model != null)
            {
                _model.CallbackException -= OnCallbackException;
                if (_model.IsOpen) _model.Close();

                _model.Dispose();
                _model = null;
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
            if (!_isDisposed && _connection?.IsOpen == true && _model?.IsOpen == true && _consumer?.IsRunning == true) return true;

            return Open();
        }

        public event EventHandler<BasicDeliverEventArgs> MessageReceived;

        private bool Open()
        {
            lock (_syncRoot)
            {
                EnsureClosed();

                Logger.Info("RabbitMQ Client is trying to connect");
                Policy.Handle<SocketException>()
                      .Or<BrokerUnreachableException>()
                      .WaitAndRetry(_retryCount,
                                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                    (ex, time) => { Logger.Warn(ex); }
                      )
                      .Execute(() => { _connection = _connectionFactory.CreateConnection(); });

                if (_connection?.IsOpen == true)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    Logger.Info($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                    _model = _connection.CreateModel();
                    _model.ExchangeDeclare(_brokerName, "direct");
                    _model.QueueDeclare(_queueName, true, false, false, null);
                    _consumer = new EventingBasicConsumer(_model);
                    _consumer.Received += OnMessageReceived;
                    _model.BasicConsume(_queueName, false, _consumer);
                    _model.CallbackException += OnCallbackException;

                    foreach (var subscribedEventName in _subscribedEventNames) _model.QueueBind(_queueName, _brokerName, subscribedEventName);

                    return true;
                }

                Logger.Error("RabbitMQ connections could not be created and opened");
                return false;
            }
        }

        public void PublishEvent(IIntegrationEvent integrationEvent)
        {
            var eventName = integrationEvent.GetType().Name;
            var message = JsonConvert.SerializeObject(integrationEvent);
            var body = Encoding.UTF8.GetBytes(message);

            Policy.Handle<BrokerUnreachableException>()
                  .Or<SocketException>()
                  .WaitAndRetry(_retryCount,
                                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                (ex, time) => { Logger.Warn(ex.ToString()); })
                  .Execute(() => _model.BasicPublish(_brokerName, eventName, null, body));
        }

        public void Subscribe(string eventName)
        {
            EnsureOpen();
            _model.QueueBind(_queueName, _brokerName, eventName);
            _subscribedEventNames.Add(eventName);
        }

        public void Unsubscribe(string eventName)
        {
            EnsureOpen();
            _model.QueueUnbind(_queueName, _brokerName, eventName);
            _subscribedEventNames.Remove(eventName);
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_isDisposed) return;

            Logger.Warn(e.Exception, "A RabbitMQ connection threw an exception.");
            Open();
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_isDisposed) return;

            Logger.Warn($"A RabbitMQ connection is blocked with reason {e.Reason}");
            Open();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_isDisposed) return;

            Logger.Warn($"A RabbitMQ connection is shut down with reason {reason}.");
            Open();
        }

        private void OnMessageReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            MessageReceived?.Invoke(this, basicDeliverEventArgs);
        }
    }
}