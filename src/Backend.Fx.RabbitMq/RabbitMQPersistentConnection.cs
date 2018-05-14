namespace Backend.Fx.RabbitMq
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using Logging;
    using Polly;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using RabbitMQ.Client.Exceptions;
    
    public class RabbitMQPersistentConnection
       : IRabbitMQPersistentConnection
    {
        private static readonly ILogger Logger = LogManager.Create<RabbitMQPersistentConnection>();
        
        private readonly IConnectionFactory connectionFactory;
        private readonly int retryCount;
        private IConnection connection;
        private bool disposed;

        private readonly object syncRoot = new object();

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            this.retryCount = retryCount;
        }

        public bool IsConnected
        {
            get
            {
                return connection != null && connection.IsOpen && !disposed;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return connection.CreateModel();
        }

        public void Dispose()
        {
            if (disposed) return;

            disposed = true;

            try
            {
                connection.Dispose();
            }
            catch (IOException ex)
            {
                Logger.Fatal(ex);
            }
        }

        public bool TryConnect()
        {
            Logger.Info("RabbitMQ Client is trying to connect");

            lock (syncRoot)
            {
                var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        Logger.Warn(ex);
                    }
                );

                policy.Execute(() =>
                {
                    connection = connectionFactory
                          .CreateConnection();
                });

                if (IsConnected)
                {
                    connection.ConnectionShutdown += OnConnectionShutdown;
                    connection.CallbackException += OnCallbackException;
                    connection.ConnectionBlocked += OnConnectionBlocked;

                    Logger.Info($"RabbitMQ persistent connection acquired a connection {connection.Endpoint.HostName} and is subscribed to failure events");

                    return true;
                }
                else
                {
                    Logger.Fatal("RabbitMQ connections could not be created and opened");

                    return false;
                }
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (disposed) return;

            Logger.Warn("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (disposed) return;

            Logger.Warn("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (disposed) return;

            Logger.Warn("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }
    }
}
