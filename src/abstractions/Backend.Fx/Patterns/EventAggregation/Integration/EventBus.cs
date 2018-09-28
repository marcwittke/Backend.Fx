namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DependencyInjection;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Extensions;
    using Logging;
    using UnitOfWork;

    public interface IEventBus : IDisposable
    {
        void Connect();

        /// <summary>
        /// Directly publishes an event on the event bus without delay.
        /// In most cases you want to publish an event when the cause is considered as safely done, e.g. when the 
        /// wrapping transaction is committed. Use <see cref="IEventBusScope"/> to let the framework raise all events
        /// after committing the unit of work.
        /// </summary>
        /// <param name="integrationEvent"></param>
        /// <returns></returns>
        Task Publish(IIntegrationEvent integrationEvent);

        /// <summary>
        /// Subscribes to an integration event with a dynamic event handler
        /// </summary>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <param name="eventName">Th eevent name to subscribe to. (Should be Type.FullName to avoid namespace collisions)</param>
        void Subscribe<THandler>(string eventName)
            where THandler : IIntegrationEventHandler;

        /// <summary>
        /// Subscribes to an integration event with a generically typed event handler
        /// </summary>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <typeparam name="TEvent">The event type to subscribe to</typeparam>
        void Subscribe<THandler, TEvent>()
                where THandler : IIntegrationEventHandler<TEvent>
                where TEvent : IIntegrationEvent;

        void Unsubscribe<THandler>(string eventName)
            where THandler : IIntegrationEventHandler;

        void Unsubscribe<THandler, TEvent>()
                where THandler : IIntegrationEventHandler<TEvent>
                where TEvent : IIntegrationEvent;
    }

    public abstract class EventBus : IEventBus
    {
        private static readonly ILogger Logger = LogManager.Create<EventBus>();
        private readonly IScopeManager _scopeManager;
        private readonly IExceptionLogger _exceptionLogger;


        /// <summary>
        /// Holds the registered handlers.
        /// Each event type name (key) matches to various handler types
        /// </summary>
        private readonly ConcurrentDictionary<string, List<Type>> _subscriptions = new ConcurrentDictionary<string, List<Type>>();

        protected EventBus(IScopeManager scopeManager, IExceptionLogger exceptionLogger)
        {
            Logger.Debug($"Instantiating {GetType()}");
            this._scopeManager = scopeManager;
            this._exceptionLogger = exceptionLogger;
        }

        public abstract void Connect();
        public abstract Task Publish(IIntegrationEvent integrationEvent);


        /// <inheritdoc />
        public void Subscribe<THandler>(string eventName) where THandler : IIntegrationEventHandler
        {
            Logger.Info($"Subscribing to {eventName}");
            var handlerType = typeof(THandler);
            _subscriptions.AddOrUpdate(eventName,
                                      s => new List<Type> { handlerType },
                                      (s, list) =>
                                      {
                                          list.Add(handlerType);
                                          return list;
                                      });
            Subscribe(eventName);
        }

        /// <inheritdoc />
        public void Subscribe<THandler, TEvent>() where THandler : IIntegrationEventHandler<TEvent> where TEvent : IIntegrationEvent
        {
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;

            Logger.Info($"Subscribing to {eventName}");
            var handlerType = typeof(THandler);
            _subscriptions.AddOrUpdate(eventName,
                                      s => new List<Type> { handlerType },
                                      (s, list) =>
                                      {
                                          list.Add(handlerType);
                                          return list;
                                      });
            Subscribe(eventName);
        }

        public void Unsubscribe<THandler>(string eventName) where THandler : IIntegrationEventHandler
        {
            Logger.Info($"Unsubscribing from {eventName}");
            if (_subscriptions.TryGetValue(eventName, out var handlers))
            {
                handlers.RemoveAll(t => t == typeof(THandler));
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
                handlers.RemoveAll(t => t == typeof(THandler));
            }
            Unsubscribe(eventName);
        }

        protected abstract void Subscribe(string eventName);
        protected abstract void Unsubscribe(string eventName);

        protected virtual void Process(string eventName, EventProcessingContext context)
        {
            Logger.Info($"Processing a {eventName} event");
            if (_subscriptions.TryGetValue(eventName, out List<Type> handlerTypes))
            {
                foreach (var handlerType in handlerTypes)
                {
                    using (var scope = _scopeManager.BeginScope(new SystemIdentity(), context.TenantId))
                    {
                        using (var unitOfWork = scope.GetInstance<IUnitOfWork>())
                        {
                            try
                            {
                                unitOfWork.Begin();
                                Logger.Info($"Getting subscribed handler instance of type {handlerType.Name}");

                                using (Logger.InfoDuration($"Invoking subscribed handler {handlerType.Name}"))
                                {

                                    if (handlerType.IsImplementationOfOpenGenericInterface(typeof(IIntegrationEventHandler<>)))
                                    {
                                        ProcessTyped(eventName, context, handlerType, scope);
                                    }
                                    else
                                    {
                                        ProcessDynamic(eventName, context, scope, handlerType);
                                    }
                                }

                                unitOfWork.Complete();
                            }
                            catch (Exception ex)
                            {
                                Logger.Info(ex, $"Handling of {eventName} by {handlerType} failed: {ex.Message}");
                                _exceptionLogger.LogException(ex);
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

        private void ProcessDynamic(string eventName, EventProcessingContext context, IScope scope, Type handlerType)
        {
            object handlerInstance = scope.GetInstance(handlerType);
            try
            {
                ((IIntegrationEventHandler) handlerInstance).Handle(context.DynamicEvent);
            }
            catch (Exception ex)
            {
                Logger.Info(ex, $"Handling of {eventName} by dynamic handler {handlerType} failed: {ex.Message}");
                _exceptionLogger.LogException(ex);
            }
        }

        private void ProcessTyped(string eventName, EventProcessingContext context, Type handlerType, IScope scope)
        {
            Type interfaceType = handlerType.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));
            var eventType = interfaceType.GetGenericArguments().Single(t => typeof(IIntegrationEvent).IsAssignableFrom(t));
            var integrationEvent = context.GetTypedEvent(eventType);
            MethodInfo handleMethod = handlerType.GetRuntimeMethod("Handle", new[] {eventType});
            Debug.Assert(handleMethod != null, $"No method with signature `Handle({eventName} event)` found on {handlerType.Name}");

            object handlerInstance = scope.GetInstance(handlerType);

            try
            {
                handleMethod.Invoke(handlerInstance, new object[] {integrationEvent});
            }
            catch (TargetInvocationException ex)
            {
                Logger.Info(ex, $"Handling of {eventName} by typed handler {handlerType} failed: {(ex.InnerException ?? ex).Message}");
                _exceptionLogger.LogException(ex.InnerException ?? ex);
            }
            catch (Exception ex)
            {
                Logger.Info(ex, $"Handling of {eventName} by typed handler {handlerType} failed: {ex.Message}");
                _exceptionLogger.LogException(ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class EventProcessingContext
    {
        public abstract TenantId TenantId { get; }
        public abstract dynamic DynamicEvent { get; }
        public abstract IIntegrationEvent GetTypedEvent(Type eventType);
    }
}
