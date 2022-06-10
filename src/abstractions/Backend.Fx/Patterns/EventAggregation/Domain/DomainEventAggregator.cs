using System;
using System.Collections.Concurrent;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.EventAggregation.Domain
{
    public class DomainEventAggregator : IDomainEventAggregator
    {
        private class HandleAction
        {
            public HandleAction(string domainEventName, string handlerTypeName, Action action)
            {
                DomainEventName = domainEventName;
                HandlerTypeName = handlerTypeName;
                Action = action;
            }

            public string DomainEventName { get; }
            public string HandlerTypeName { get; }
            public Action Action { get; }
        }

        private static readonly ILogger Logger = Log.Create<DomainEventAggregator>();
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentQueue<HandleAction> _handleActions = new ConcurrentQueue<HandleAction>();

        public DomainEventAggregator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Publish a domain event that is handled by all handlers synchronously in the same scope/transaction.
        /// Possible exceptions are not caught, so that your action might fail due to a failing event handler.
        /// </summary>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <param name="domainEvent"></param>
        public void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            foreach (var injectedHandler in _serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>())
            {
                var handleAction = new HandleAction(
                    typeof(TDomainEvent).Name,
                    injectedHandler.GetType().Name,
                    () => injectedHandler.Handle(domainEvent));

                _handleActions.Enqueue(handleAction);
                Logger.LogDebug(
                    "Invocation of {HandlerTypeName} for domain event {DomainEvent} registered. It will be executed on completion of operation",
                    injectedHandler.GetType().Name,
                    domainEvent);
            }
        }

        public void RaiseEvents()
        {
            while (_handleActions.TryDequeue(out HandleAction handleAction))
            {
                try
                {
                    handleAction.Action.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,
                        "Handling of {DomainEvent} by {HandlerTypeName} failed",
                        handleAction.DomainEventName,
                        handleAction.HandlerTypeName);
                    throw;
                }
            }
        }
    }
}