namespace Backend.Fx.RabbitMq
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Patterns.DependencyInjection;
    using Patterns.PubSub;
    using Polly;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using RabbitMQ.Client.Exceptions;

    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<RabbitMQEventBus>();

        private readonly IRabbitMQPersistentConnection persistentConnection;
        private readonly IEventBusSubscriptionsManager eventBusSubscriptionsManager;
        private readonly IScopeManager scopeManager;
        private readonly int retryCount;
        private readonly string queueName;
        private readonly string brokerName;

        private IModel consumerChannel;

        public RabbitMQEventBus(IRabbitMQPersistentConnection persistentConnection,
                                IEventBusSubscriptionsManager subsManager,
                                IScopeManager scopeManager,
                                string brokerName = null,
                                string queueName = null,
                                int retryCount = 5)
        {
            this.persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            this.eventBusSubscriptionsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            this.scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
            this.brokerName = brokerName ?? throw new ArgumentNullException(nameof(brokerName));
            this.queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            this.retryCount = retryCount;
            this.eventBusSubscriptionsManager.OnEventRemoved += EventBusSubscriptionsManagerOnEventRemoved;
        }

        public void Connect()
        {
            consumerChannel = CreateConsumerChannel();
        }

        private void EventBusSubscriptionsManagerOnEventRemoved(object sender, string eventName)
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            using (var channel = persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: queueName,
                    exchange: brokerName,
                    routingKey: eventName);

                if (eventBusSubscriptionsManager.IsEmpty)
                {
                    consumerChannel?.Close();
                }
            }
        }

        public void Publish(IntegrationEvent integrationEvent)
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    Logger.Warn(ex.ToString());
                });

            using (var channel = persistentConnection.CreateModel())
            {
                var eventName = integrationEvent.GetType().Name;
                var message = JsonConvert.SerializeObject(integrationEvent);
                var body = Encoding.UTF8.GetBytes(message);

                channel.ExchangeDeclare(exchange: brokerName, type: "direct");
                policy.Execute(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    channel.BasicPublish(exchange: brokerName, routingKey: eventName, basicProperties: null, body: body);
                });
            }
        }

        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            DoInternalSubscription(eventName);
            eventBusSubscriptionsManager.AddDynamicSubscription<TH>(eventName);
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = eventBusSubscriptionsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);
            eventBusSubscriptionsManager.AddSubscription<T, TH>();
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = eventBusSubscriptionsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!persistentConnection.IsConnected)
                {
                    persistentConnection.TryConnect();
                }

                using (var channel = persistentConnection.CreateModel())
                {
                    channel.QueueBind(queue: queueName,
                                      exchange: brokerName,
                                      routingKey: eventName);
                }
            }
        }

        public void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            eventBusSubscriptionsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            eventBusSubscriptionsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            consumerChannel?.Dispose();
            eventBusSubscriptionsManager.OnEventRemoved -= EventBusSubscriptionsManagerOnEventRemoved;
            eventBusSubscriptionsManager.Clear();
        }

        private IModel CreateConsumerChannel()
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            var channel = persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: brokerName,
                                 type: "direct");

            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body);

                await ProcessEvent(eventName, message);
            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

            channel.CallbackException += (sender, ea) =>
            {

                consumerChannel?.Dispose();
                consumerChannel = CreateConsumerChannel();
            };

            return channel;
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            dynamic eventData = JObject.Parse(message);
            if (eventBusSubscriptionsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = scopeManager.BeginScope(new SystemIdentity(), new TenantId(eventData.tenantId)))
                {
                    var subscriptions = eventBusSubscriptionsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            IDynamicIntegrationEventHandler handler = (IDynamicIntegrationEventHandler)scope.GetInstance(subscription.HandlerType);
                            await handler.Handle(eventData);
                        }
                        else
                        {
                            var eventType = eventBusSubscriptionsManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                            var handler = scope.GetInstance(subscription.HandlerType);
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] { integrationEvent });
                        }
                    }
                }
            }
        }
    }
}
