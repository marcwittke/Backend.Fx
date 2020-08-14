using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        private IBackendFxApplication _application;

        public abstract void Connect();

        public void IntegrateApplication(IBackendFxApplication application)
        {
            _application = application;
        }

        public Task Publish(IIntegrationEvent integrationEvent)
        {
            return PublishOnMessageBus(integrationEvent);
        }

        protected abstract Task PublishOnMessageBus(IIntegrationEvent integrationEvent);


        /// <inheritdoc />
        public void Subscribe<THandler>(string eventName) where THandler : IIntegrationMessageHandler
        {
            Logger.Info($"Subscribing to {eventName}");
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
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
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
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
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

        public void Unsubscribe<THandler>(string eventName) where THandler : IIntegrationMessageHandler
        {
            Logger.Info($"Unsubscribing from {eventName}");
            if (_subscriptions.TryGetValue(eventName, out var handlers))
            {
                handlers.RemoveAll(t => t.Matches(typeof(THandler)));
            }

            Unsubscribe(eventName);
        }

        public void Unsubscribe<THandler, TEvent>() where THandler : IIntegrationMessageHandler<TEvent> where TEvent : IIntegrationEvent
        {
            string eventName = typeof(TEvent).FullName;
            Debug.Assert(eventName != null, nameof(eventName) + " != null");

            Logger.Info($"Unsubscribing from {eventName}");
            if (_subscriptions.TryGetValue(eventName, out var handlers))
            {
                handlers.RemoveAll(t => t.Matches(typeof(THandler)));
            }

            Unsubscribe(eventName);
        }

        public void Unsubscribe<TEvent>(IIntegrationMessageHandler<TEvent> handler) where TEvent : IIntegrationEvent
        {
            string eventName = typeof(TEvent).FullName;
            Debug.Assert(eventName != null, nameof(eventName) + " != null");

            Logger.Info($"Unsubscribing from {eventName}");
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
            Logger.Info($"Processing a {eventName} event");
            EnsureInvoker();

            if (_subscriptions.TryGetValue(eventName, out List<ISubscription> subscriptions))
            {
                foreach (ISubscription subscription in subscriptions)
                {
                    _application.WaitForBoot();
                    _application.Invoker.Invoke(
                        instanceProvider => subscription.Process(instanceProvider, context),
                        new SystemIdentity(),
                        context.TenantId,
                        context.CorrelationId);
                }
            }
            else
            {
                Logger.Info($"No handler registered. Ignoring {eventName} event");
            }
        }

        private void EnsureInvoker()
        {
            if (_application == null)
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
    }
}