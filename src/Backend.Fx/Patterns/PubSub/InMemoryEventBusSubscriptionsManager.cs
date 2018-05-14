namespace Backend.Fx.Patterns.PubSub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {


        private readonly Dictionary<string, List<SubscriptionInfo>> handlers;
        private readonly List<Type> eventTypes;

        public event EventHandler<string> OnEventRemoved;

        public InMemoryEventBusSubscriptionsManager()
        {
            handlers = new Dictionary<string, List<SubscriptionInfo>>();
            eventTypes = new List<Type>();
        }

        public bool IsEmpty
        {
            get { return !handlers.Keys.Any(); }
        }

        public void Clear()
        {
            handlers.Clear();
        }

        public void AddDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            DoAddSubscription(typeof(TH), eventName, isDynamic: true);
        }

        public void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            DoAddSubscription(typeof(TH), eventName, isDynamic: false);
            eventTypes.Add(typeof(T));
        }

        private void DoAddSubscription(Type handlerType, string eventName, bool isDynamic)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            handlers[eventName].Add(isDynamic ? SubscriptionInfo.Dynamic(handlerType) : SubscriptionInfo.Typed(handlerType));
        }


        public void RemoveDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            var handlerToRemove = FindDynamicSubscriptionToRemove<TH>(eventName);
            DoRemoveHandler(eventName, handlerToRemove);
        }


        public void RemoveSubscription<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            DoRemoveHandler(eventName, handlerToRemove);
        }


        private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                handlers[eventName].Remove(subsToRemove);
                if (!handlers[eventName].Any())
                {
                    handlers.Remove(eventName);
                    var eventType = eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(eventName);
                }

            }
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => handlers[eventName];

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            if (handler != null)
            {
                OnEventRemoved?.Invoke(this, eventName);
            }
        }


        private SubscriptionInfo FindDynamicSubscriptionToRemove<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }


        private SubscriptionInfo FindSubscriptionToRemove<T, TH>()
             where T : IntegrationEvent
             where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }

        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);

        }

        public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubscriptionsForEvent(key);
        }
        public bool HasSubscriptionsForEvent(string eventName) => handlers.ContainsKey(eventName);

        public Type GetEventTypeByName(string eventName) => eventTypes.SingleOrDefault(t => t.Name == eventName);

        public string GetEventKey<T>()
        {
            return typeof(T).Name;
        }
    }
}
