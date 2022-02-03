using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
            if (_invoker != null && !Equals(_invoker, invoker))
            {
                throw new InvalidOperationException("This message bus instance has been linked to an application instance invoker before. " +
                                                    "You cannot share the same message bus instance between multiple applications.");
            }
            _invoker = invoker;
        }

        public Task Publish(IIntegrationEvent integrationEvent)
        {
            return PublishOnMessageBus(integrationEvent);
        }

        protected abstract Task PublishOnMessageBus(IIntegrationEvent integrationEvent);


        /// <inheritdoc />
        public void Subscribe<THandler>(string eventName) where THandler : IIntegrationMessageHandler
        {
            Logger.LogInformation("Subscribing to {EventName}", eventName);
            EnsureInvoker();
            var subscription = new DynamicSubscription(typeof(THandler));
            _subscriptions.AddOrUpdate(eventName,
                                       s => new List<ISubscription> {subscription},
                                       (s, list) =>
                                       {
                                           list.Add(subscription);
                                           return list;
                                       });
            Subscribe(eventName);
        }

        /// <inheritdoc />
        public void Subscribe<THandler, TEvent>() where THandler : IIntegrationMessageHandler<TEvent> where TEvent : IIntegrationEvent
        {
            string eventName = MessageNameProvider.GetMessageName<TEvent>();
            Logger.LogInformation("Subscribing to {EventName}", eventName);
            EnsureInvoker();
            var subscription = new TypedSubscription(typeof(THandler), typeof(TEvent));
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
            Logger.LogInformation("Subscribing to {EventName}", eventName);
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

        public void Unsubscribe<THandler>(string eventName) where THandler : IIntegrationMessageHandler
        {
            Logger.LogInformation("Unsubscribing from {EventName}", eventName);
            if (_subscriptions.TryGetValue(eventName, out var handlers))
            {
                handlers.RemoveAll(t => t.Matches(typeof(THandler)));
            }

            Unsubscribe(eventName);
        }

        public void Unsubscribe<THandler, TEvent>() where THandler : IIntegrationMessageHandler<TEvent> where TEvent : IIntegrationEvent
        {
            string eventName = MessageNameProvider.GetMessageName<TEvent>();
            Logger.LogInformation("Unsubscribing from {EventName}", eventName);
            if (_subscriptions.TryGetValue(eventName, out var handlers))
            {
                handlers.RemoveAll(t => t.Matches(typeof(THandler)));
            }

            Unsubscribe(eventName);
        }

        public void Unsubscribe<TEvent>(IIntegrationMessageHandler<TEvent> handler) where TEvent : IIntegrationEvent
        {
            string eventName = MessageNameProvider.GetMessageName<TEvent>();
            Logger.LogInformation("Unsubscribing from {EventName}", eventName);
            if (_subscriptions.TryGetValue(eventName, out var handlers))
            {
                handlers.RemoveAll(t => t.Matches(handler));
            }

            Unsubscribe(eventName);
        }

        protected abstract void Subscribe(string eventName);
        protected abstract void Unsubscribe(string eventName);

        protected void Process(string eventName, EventProcessingContext context)
        {
            Logger.LogInformation("Processing a {EventName} message", eventName);
            EnsureInvoker();

            if (_subscriptions.TryGetValue(eventName, out List<ISubscription> subscriptions))
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
                        Logger.LogWarning(ex, "Processing a {EventName} message failed", eventName);
                        throw;
                    }
                }
            }
            else
            {
                Logger.LogInformation("No handler registered. Ignoring {EventName} event", eventName);
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
                var messageName = t.Name;
                return messageName;
            }

            public string GetMessageName(IIntegrationEvent integrationEvent)
            {
                return GetMessageName(integrationEvent.GetType());
            }
        }
    }
}