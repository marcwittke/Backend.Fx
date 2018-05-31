namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DependencyInjection;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Logging;
    using UnitOfWork;

    public interface IEventBus : IDisposable
    {
        void Connect();

        void Publish(IntegrationEvent integrationEvent);

        void Subscribe<THandler>(string eventName)
            where THandler : IIntegrationEventHandler;

        void Unsubscribe<THandler>(string eventName)
            where THandler : IIntegrationEventHandler;
    }

    public abstract class EventBus : IEventBus
    {
        private static readonly ILogger Logger = LogManager.Create<EventBus>();
        private readonly IScopeManager scopeManager;
        private readonly ConcurrentDictionary<string, List<Type>> subscriptions = new ConcurrentDictionary<string, List<Type>>();

        protected EventBus(IScopeManager scopeManager)
        {
            Logger.Debug($"Instantiating {GetType()}");
            this.scopeManager = scopeManager;
        }

        public abstract void Connect();
        public abstract void Publish(IntegrationEvent integrationEvent);

        public void Subscribe<THandler>(string eventName) where THandler : IIntegrationEventHandler
        {
            Logger.Info($"Subscribing to {eventName}");
            var type = typeof(THandler);
            subscriptions.AddOrUpdate(eventName,
                                      s => new List<Type> { type },
                                      (s, list) =>
                                      {
                                          list.Add(type);
                                          return list;
                                      });
            Subscribe(eventName);
        }

        public void Unsubscribe<THandler>(string eventName) where THandler : IIntegrationEventHandler
        {
            Logger.Info($"Unsubscribing from {eventName}");
            subscriptions.TryRemove(eventName, out var _);
            Unsubscribe(eventName);
        }

        protected abstract void Subscribe(string eventName);
        protected abstract void Unsubscribe(string eventName);
        protected abstract IntegrationEventData Deserialize(object rawEventPayload);

        protected virtual async Task Process(string eventName, object rawEventPayload)
        {
            Logger.Info($"Processing a {eventName} event");
            if (subscriptions.TryGetValue(eventName, out List<Type> handlerTypes))
            {
                IntegrationEventData integrationEvent = Deserialize(rawEventPayload);
                foreach (var handlerType in handlerTypes)
                {
                    using (var scope = scopeManager.BeginScope(new SystemIdentity(), integrationEvent.TenantId))
                    {
                        using (var unitOfWork = scope.GetInstance<IUnitOfWork>())
                        {
                            try
                            {
                                unitOfWork.Begin();
                                Logger.Info($"Getting subscribed handler instance of type {handlerType.Name}");
                                var handler = (IIntegrationEventHandler)scope.GetInstance(handlerType);
                                using (Logger.InfoDuration($"Invoking subscribed handler {handler.GetType().Name}"))
                                {
                                    await handler.Handle(integrationEvent.Event);
                                }
                                unitOfWork.Complete();
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, $"Handling of {eventName} by {handlerType} failed: {ex.Message}");
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.Info($"No handler registered. Ignoring {eventName} event");
            }
        }

        protected class IntegrationEventData
        {
            public IntegrationEventData(TenantId tenantId, dynamic @event)
            {
                TenantId = tenantId;
                Event = @event;
            }

            public TenantId TenantId { get; }
            public dynamic Event { get; set; }
        }

        protected virtual void Dispose(bool disposing)
        {}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
