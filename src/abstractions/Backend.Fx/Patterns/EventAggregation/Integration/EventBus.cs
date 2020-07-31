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
    public abstract class EventBus : IEventBus
    {
        private static readonly ILogger Logger = LogManager.Create<EventBus>();
        private readonly IBackendFxApplication _application;

        /// <summary>
        /// Holds the registered handlers.
        /// Each event type name (key) matches to various subscriptions
        /// </summary>
        private readonly ConcurrentDictionary<string, List<ISubscription>> _subscriptions = new ConcurrentDictionary<string, List<ISubscription>>();

        protected EventBus(IBackendFxApplication application)
        {
            Logger.Debug($"Instantiating {GetType()}");
            _application = application;
        }

        public abstract void Connect();

        public Task Publish(IIntegrationEvent integrationEvent)
        {
            Guid correlationId = _application.CompositionRoot.TryGetCurrentCorrelation(out Correlation correlation) ? correlation.Id : Guid.NewGuid();
            ((IntegrationEvent) integrationEvent).SetCorrelationId(correlationId);
            return PublishOnEventBus(integrationEvent);
        }

        protected abstract Task PublishOnEventBus(IIntegrationEvent integrationEvent);


        /// <inheritdoc />
        public void Subscribe<THandler>(string eventName) where THandler : IIntegrationEventHandler
        {
            Logger.Info($"Subscribing to {eventName}");
            var subscription = new DynamicSubscription(_application, typeof(THandler));
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
        public void Subscribe<THandler, TEvent>() where THandler : IIntegrationEventHandler<TEvent> where TEvent : IIntegrationEvent
        {
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;

            Logger.Info($"Subscribing to {eventName}");
            var subscription = new TypedSubscription(_application, typeof(THandler));
            _subscriptions.AddOrUpdate(eventName,
                                       s => new List<ISubscription> {subscription},
                                       (s, list) =>
                                       {
                                           list.Add(subscription);
                                           return list;
                                       });
            Subscribe(eventName);
        }

        public void Subscribe<TEvent>(IIntegrationEventHandler<TEvent> handler)
            where TEvent : IIntegrationEvent
        {
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;

            Logger.Info($"Subscribing to {eventName}");
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

        public void Unsubscribe<THandler>(string eventName) where THandler : IIntegrationEventHandler
        {
            Logger.Info($"Unsubscribing from {eventName}");
            if (_subscriptions.TryGetValue(eventName, out var handlers))
            {
                handlers.RemoveAll(t => t.Matches(typeof(THandler)));
            }

            Unsubscribe(eventName);
        }

        public void Unsubscribe<THandler, TEvent>() where THandler : IIntegrationEventHandler<TEvent> where TEvent : IIntegrationEvent
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

        public void Unsubscribe<TEvent>(IIntegrationEventHandler<TEvent> handler) where TEvent : IIntegrationEvent
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
            if (_subscriptions.TryGetValue(eventName, out List<ISubscription> subscriptions))
            {
                foreach (ISubscription subscription in subscriptions)
                {
                    _application.Invoke(
                        () => subscription.Process(eventName, context),
                        new SystemIdentity(),
                        context.TenantId,
                        compositionRoot => ConfigureProcessingScope(compositionRoot, context));
                }
            }
            else
            {
                Logger.Info($"No handler registered. Ignoring {eventName} event");
            }
        }

        protected virtual void ConfigureProcessingScope(ICompositionRoot compositionRoot, EventProcessingContext context)
        {
            compositionRoot.GetInstance<ICurrentTHolder<Correlation>>().Current.Resume(context.CorrelationId);
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