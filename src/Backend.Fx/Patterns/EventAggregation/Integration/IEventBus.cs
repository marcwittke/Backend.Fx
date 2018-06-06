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
        private readonly IScopeManager scopeManager;
        private readonly IExceptionLogger exceptionLogger;


        /// <summary>
        /// Holds the registered handlers.
        /// Each event type name (key) matches to various handler types
        /// </summary>
        private readonly ConcurrentDictionary<string, List<Type>> subscriptions = new ConcurrentDictionary<string, List<Type>>();

        protected EventBus(IScopeManager scopeManager, IExceptionLogger exceptionLogger)
        {
            Logger.Debug($"Instantiating {GetType()}");
            this.scopeManager = scopeManager;
            this.exceptionLogger = exceptionLogger;
        }

        public abstract void Connect();
        public abstract Task Publish(IIntegrationEvent integrationEvent);


        /// <inheritdoc />
        public void Subscribe<THandler>(string eventName) where THandler : IIntegrationEventHandler
        {
            Logger.Info($"Subscribing to {eventName}");
            var handlerType = typeof(THandler);
            subscriptions.AddOrUpdate(eventName,
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
            string eventName = typeof(TEvent).FullName;

            Logger.Info($"Subscribing to {eventName}");
            var handlerType = typeof(THandler);
            subscriptions.AddOrUpdate(eventName,
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
            if (subscriptions.TryGetValue(eventName, out var handlers))
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
            if (subscriptions.TryGetValue(eventName, out var handlers))
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
            if (subscriptions.TryGetValue(eventName, out List<Type> handlerTypes))
            {
                foreach (var handlerType in handlerTypes)
                {
                    using (var scope = scopeManager.BeginScope(new SystemIdentity(), context.TenantId))
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
                                        Type interfaceType = handlerType.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));
                                        var eventType = interfaceType.GetGenericArguments().Single(t => typeof(IIntegrationEvent).IsAssignableFrom(t));
                                        var integrationEvent = context.GetTypedEvent(eventType);
                                        MethodInfo handleMethod = handlerType.GetRuntimeMethod("Handle", new[] { eventType });
                                        Debug.Assert(handleMethod != null, $"No method with signature `Handle({eventName} event)` found on {handlerType.Name}");

                                        object handlerInstance = scope.GetInstance(handlerType);

                                        try
                                        {
                                            handleMethod.Invoke(handlerInstance, new object[] { integrationEvent });
                                        }
                                        catch (TargetInvocationException ex)
                                        {
                                            Logger.Info(ex, $"Handling of {eventName} by typed handler {handlerType} failed: {(ex.InnerException ?? ex).Message}");
                                            exceptionLogger.LogException(ex.InnerException ?? ex);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Info(ex, $"Handling of {eventName} by typed handler {handlerType} failed: {ex.Message}");
                                            exceptionLogger.LogException(ex);
                                        }
                                    }
                                    else
                                    {
                                        object handlerInstance = scope.GetInstance(handlerType);
                                        try
                                        {
                                            ((IIntegrationEventHandler)handlerInstance).Handle(context.DynamicEvent);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Info(ex, $"Handling of {eventName} by dynamic handler {handlerType} failed: {ex.Message}");
                                            exceptionLogger.LogException(ex);
                                        }
                                    }
                                }

                                unitOfWork.Complete();
                            }
                            catch (Exception ex)
                            {
                                Logger.Info(ex, $"Handling of {eventName} by {handlerType} failed: {ex.Message}");
                                exceptionLogger.LogException(ex);
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
