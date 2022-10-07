using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.DomainEvents
{
    public class DomainEventAggregator : IDomainEventAggregator, IDomainEventAggregatorScope
    {
        private class HandleAction
        {
            public HandleAction(string domainEventName, string handlerTypeName, Func<Task> asyncAction)
            {
                DomainEventName = domainEventName;
                HandlerTypeName = handlerTypeName;
                AsyncAction = asyncAction;
            }

            public string DomainEventName { get; }
            public string HandlerTypeName { get; }
            public Func<Task> AsyncAction { get; }
        }

        private static readonly ILogger Logger = Log.Create<DomainEventAggregator>();
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentQueue<HandleAction> _handleActions = new();

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
                    async () => await injectedHandler.HandleAsync(domainEvent).ConfigureAwait(false));

                _handleActions.Enqueue(handleAction);
                Logger.LogDebug(
                    "Invocation of {HandlerTypeName} for domain event {DomainEvent} registered. It will be executed on completion of operation",
                    injectedHandler.GetType().Name,
                    domainEvent);
            }
        }

        public async Task RaiseEventsAsync()
        {
            while (_handleActions.TryDequeue(out HandleAction handleAction))
            {
                try
                {
                    var task = handleAction.AsyncAction.Invoke();
                    await task.ConfigureAwait(false);
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