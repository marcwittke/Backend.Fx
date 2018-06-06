namespace Backend.Fx.RabbitMq
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using Logging;
    using Newtonsoft.Json;
    using Patterns.EventAggregation.Integration;
    using Polly;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using RabbitMQ.Client.Exceptions;

    public class RabbitMQChannel : IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<RabbitMQChannel>();
        private readonly string brokerName;
        private readonly IConnectionFactory connectionFactory;
        private readonly string queueName;
        private readonly int retryCount;
        private readonly object syncRoot = new object();
        private readonly HashSet<string> subscribedEventNames = new HashSet<string>();

        private IConnection connection;
        private EventingBasicConsumer consumer;
        private bool isDisposed;
        private IModel model;

        public RabbitMQChannel(IConnectionFactory connectionFactory, string brokerName, string queueName, int retryCount)
        {
            this.connectionFactory = connectionFactory;
            this.brokerName = brokerName;
            this.queueName = queueName;
            this.retryCount = retryCount;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

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
            if (consumer != null)
            {
                consumer.Received -= OnMessageReceived;
                consumer = null;
            }

            if (model != null)
            {
                model.CallbackException -= OnCallbackException;
                if (model.IsOpen)
                {
                    model.Close();
                }

                model.Dispose();
                model = null;
            }

            if (connection != null)
            {
                connection.ConnectionShutdown -= OnConnectionShutdown;
                connection.CallbackException -= OnCallbackException;
                connection.ConnectionBlocked -= OnConnectionBlocked;
                if (connection.IsOpen)
                {
                    connection.Close();
                }

                connection.Dispose();
                connection = null;
            }
        }

        public bool EnsureOpen()
        {
            if (!isDisposed && connection?.IsOpen == true && model?.IsOpen == true && consumer?.IsRunning == true)
            {
                return true;
            }

            return Open();
        }

        public event EventHandler<BasicDeliverEventArgs> MessageReceived;

        private bool Open()
        {
            lock (syncRoot)
            {
                EnsureClosed();

                Logger.Info("RabbitMQ Client is trying to connect");
                Policy.Handle<SocketException>()
                      .Or<BrokerUnreachableException>()
                      .WaitAndRetry(retryCount,
                                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                    (ex, time) => { Logger.Warn(ex); }
                      )
                      .Execute(() => { connection = connectionFactory.CreateConnection(); });

                if (connection?.IsOpen == true)
                {
                    connection.ConnectionShutdown += OnConnectionShutdown;
                    connection.CallbackException += OnCallbackException;
                    connection.ConnectionBlocked += OnConnectionBlocked;

                    Logger.Info($"RabbitMQ persistent connection acquired a connection {connection.Endpoint.HostName} and is subscribed to failure events");

                    model = connection.CreateModel();
                    model.ExchangeDeclare(brokerName, "direct");
                    model.QueueDeclare(queueName, true, false, false, null);
                    consumer = new EventingBasicConsumer(model);
                    consumer.Received += OnMessageReceived;
                    model.BasicConsume(queueName, false, consumer);
                    model.CallbackException += OnCallbackException;

                    foreach (var subscribedEventName in subscribedEventNames)
                    {
                        model.QueueBind(queueName, brokerName, subscribedEventName);
                    }

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
                  .WaitAndRetry(retryCount,
                                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                (ex, time) => { Logger.Warn(ex.ToString()); })
                  .Execute(() => model.BasicPublish(brokerName, eventName, null, body));
        }

        public void Subscribe(string eventName)
        {
            EnsureOpen();
            model.QueueBind(queueName, brokerName, eventName);
            subscribedEventNames.Add(eventName);
        }

        public void Unsubscribe(string eventName)
        {
            EnsureOpen();
            model.QueueUnbind(queueName, brokerName, eventName);
            subscribedEventNames.Remove(eventName);
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (isDisposed)
            {
                return;
            }

            Logger.Warn(e.Exception, "A RabbitMQ connection threw an exception.");
            Open();
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (isDisposed)
            {
                return;
            }

            Logger.Warn($"A RabbitMQ connection is blocked with reason {e.Reason}");
            Open();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (isDisposed)
            {
                return;
            }

            Logger.Warn($"A RabbitMQ connection is shut down with reason {reason}.");
            Open();
        }

        private void OnMessageReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            MessageReceived?.Invoke(this, basicDeliverEventArgs);
        }
    }
}