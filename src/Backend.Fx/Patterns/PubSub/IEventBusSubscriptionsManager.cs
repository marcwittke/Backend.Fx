namespace Backend.Fx.Patterns.PubSub
{
    using System;
    using System.Collections.Generic;

    public interface IEventBusSubscriptionsManager
    {
        bool IsEmpty { get; }

        void AddDynamicSubscription<TH>(string eventName)
                where TH : IDynamicIntegrationEventHandler;

        void AddSubscription<T, TH>()
                where T : IntegrationEvent
                where TH : IIntegrationEventHandler<T>;

        void Clear();
        string GetEventKey<T>();
        Type GetEventTypeByName(string eventName);
        IEnumerable<InMemoryEventBusSubscriptionsManager.SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
        IEnumerable<InMemoryEventBusSubscriptionsManager.SubscriptionInfo> GetHandlersForEvent(string eventName);

        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;
        bool HasSubscriptionsForEvent(string eventName);
        event EventHandler<string> OnEventRemoved;

        void RemoveDynamicSubscription<TH>(string eventName)
                where TH : IDynamicIntegrationEventHandler;

        void RemoveSubscription<T, TH>()
                where TH : IIntegrationEventHandler<T>
                where T : IntegrationEvent;
    }
}