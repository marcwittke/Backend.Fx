namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DependencyInjection;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using UnitOfWork;

    public interface IEventBus
    {
        void Publish(IntegrationEvent integrationEvent);

        void Subscribe<THandler>(string eventName)
            where THandler : IIntegrationEventHandler;

        void Unsubscribe<THandler>(string eventName)
            where THandler : IIntegrationEventHandler;
    }

    public abstract class EventBus : IEventBus
    {
        private readonly IScopeManager scopeManager;
        private readonly ConcurrentDictionary<string, List<Type>> subscriptions = new ConcurrentDictionary<string, List<Type>>();

        protected EventBus(IScopeManager scopeManager)
        {
            this.scopeManager = scopeManager;
        }

        public abstract void Publish(IntegrationEvent integrationEvent);

        public void Subscribe<THandler>(string eventName) where THandler : IIntegrationEventHandler
        {
            var type = typeof(THandler);
            subscriptions.AddOrUpdate(eventName, 
                                      s => new List<Type> { type }, 
                                      (s, list) => { 
                                          list.Add(type);
                                          return list;
                                      });
            Subscribe(eventName);
        }

        public void Unsubscribe<THandler>(string eventName) where THandler : IIntegrationEventHandler
        {
            subscriptions.TryRemove(eventName, out var _);
            Unsubscribe(eventName);
        }

        protected abstract void Subscribe(string eventName);
        protected abstract void Unsubscribe(string eventName);
        protected abstract IntegrationEventData Deserialize(object rawEventPayload);

        protected virtual async Task Process(string eventName, object rawEventPayload)
        {
            if (subscriptions.TryGetValue(eventName, out List<Type> handlerTypes))
            {
                IntegrationEventData integrationEvent = Deserialize(rawEventPayload);
                foreach (var handlerType in handlerTypes)
                {
                    using (var scope = scopeManager.BeginScope(new SystemIdentity(), integrationEvent.TenantId))
                    {
                        using (var unitOfWork = scope.GetInstance<IUnitOfWork>())
                        {
                            unitOfWork.Begin();

                            var dynamicHandler = (IIntegrationEventHandler)scope.GetInstance(handlerType);
                            await dynamicHandler.Handle(integrationEvent.Event);

                            unitOfWork.Complete();
                        }
                    }
                }
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
    }
}
