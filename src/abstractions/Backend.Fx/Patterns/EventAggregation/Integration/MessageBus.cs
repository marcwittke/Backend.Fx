using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public abstract class MessageBus : IMessageBus
    {
        private static readonly ILogger Logger = LogManager.Create<MessageBus>();

        /// <summary>
        /// Holds the registered handlers.
        /// Each event type name (key) matches to various subscriptions
        /// </summary>
        private readonly ConcurrentDictionary<string, List<ISubscription>> _subscriptions = new ConcurrentDictionary<string, List<ISubscription>>();

        private IBackendFxApplicationInvoker _invoker;

        public IMessageNameProvider MessageNameProvider { get; } = new DefaultMessageNameProvider();
        public abstract void Connect();

        public void ProvideInvoker(IBackendFxApplicationInvoker invoker)
        {
            _invoker = invoker;
        }

        public Task Publish(IIntegrationEvent integrationEvent)
        {
            return PublishOnMessageBus(integrationEvent);
        }

        protected abstract Task PublishOnMessageBus(IIntegrationEvent integrationEvent);


        /// <inheritdoc />
        public void Subscribe<THandler>(string messageName) where THandler : IIntegrationMessageHandler
        {
            Logger.Info($"Subscribing to {messageName}");
            EnsureInvoker();
            var subscription = new DynamicSubscription(typeof(THandler));
            _subscriptions.AddOrUpdate(messageName,
                                       s => new List<ISubscription> {subscription},
                                       (s, list) =>
                                       {
                                           list.Add(subscription);
                                           return list;
                                       });
            Subscribe(messageName);
        }

        /// <inheritdoc />
        public void Subscribe<THandler, TEvent>() where THandler : IIntegrationMessageHandler<TEvent> where TEvent : IIntegrationEvent
        {
            string eventName = MessageNameProvider.GetMessageName<TEvent>();
            Logger.Info($"Subscribing to {eventName}");
            EnsureInvoker();
            var subscription = new TypedSubscription(typeof(THandler));
            _subscriptions.AddOrUpdate(eventName,
                                       s => new List<ISubscription> {subscription},
                                       (s, list) =>
                                       {
                                           list.Add(subscription);
                                           return list;
                                       });
            Subscribe(eventName);
        }

        public void Subscribe<TEvent>(IIntegrationMessageHandler<TEvent> handler)
            where TEvent : IIntegrationEvent
        {
            string eventName = MessageNameProvider.GetMessageName<TEvent>();
            Logger.Info($"Subscribing to {eventName}");
            EnsureInvoker();
            var subscription = new SingletonSubscription<TEvent>(handler);
            _subscriptions.AddOrUpdate(eventName,
                                       s => new List<ISubscription> {subscription},
                                       (s, list) =>
                                       {
                                           list.Add(subscription);
                                           return list;
                                       });
            Subscribe(eventName);
        }

        public void Unsubscribe<THandler>(string messageName) where THandler : IIntegrationMessageHandler
        {
            Logger.Info($"Unsubscribing from {messageName}");
            if (_subscriptions.TryGetValue(messageName, out var handlers))
            {
                handlers.RemoveAll(t => t.Matches(typeof(THandler)));
            }

            Unsubscribe(messageName);
        }

        public void Unsubscribe<THandler, TEvent>() where THandler : IIntegrationMessageHandler<TEvent> where TEvent : IIntegrationEvent
        {
            string eventName = MessageNameProvider.GetMessageName<TEvent>();
            Logger.Info($"Unsubscribing from {eventName}");
            if (_subscriptions.TryGetValue(eventName, out var handlers))
            {
                handlers.RemoveAll(t => t.Matches(typeof(THandler)));
            }

            Unsubscribe(eventName);
        }

        public void Unsubscribe<TEvent>(IIntegrationMessageHandler<TEvent> handler) where TEvent : IIntegrationEvent
        {
            string eventName = MessageNameProvider.GetMessageName<TEvent>();
            Logger.Info($"Unsubscribing from {eventName}");
            if (_subscriptions.TryGetValue(eventName, out var handlers))
            {
                handlers.RemoveAll(t => t.Matches(handler));
            }

            Unsubscribe(eventName);
        }

        protected abstract void Subscribe(string messageName);
        protected abstract void Unsubscribe(string messageName);

        protected void Process(string messageName, EventProcessingContext context)
        {
            Logger.Info($"Processing a {messageName} message");
            EnsureInvoker();

            if (_subscriptions.TryGetValue(messageName, out List<ISubscription> subscriptions))
            {
                foreach (ISubscription subscription in subscriptions)
                {
                    try
                    {
                        _invoker.Invoke(
                            instanceProvider => subscription.Process(instanceProvider, context),
                            new SystemIdentity(),
                            context.TenantId,
                            context.CorrelationId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"Processing a {messageName} message failed");
                        throw;
                    }
                }
            }
            else
            {
                Logger.Info($"No handler registered. Ignoring {messageName} event");
            }
        }

        private void EnsureInvoker()
        {
            if (_invoker == null)
            {
                throw new InvalidOperationException("Before using the message bus you have to provide the application invoker by calling ProvideInvoker()");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private class DefaultMessageNameProvider : IMessageNameProvider
        {
            public string GetMessageName<T>()
            {
                return GetMessageName(typeof(T));
            }

            public string GetMessageName(Type t)
            {
                var messageName = t.Name ?? throw new ArgumentException("Type name is null!");
                return messageName;
            }

            public string GetMessageName(IIntegrationEvent integrationEvent)
            {
                return GetMessageName(integrationEvent.GetType());
            }
        }
    }
}