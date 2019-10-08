using System.Threading.Tasks;

namespace Backend.Fx.Patterns.EventAggregation.Domain
{
    using System;
    using System.Collections.Concurrent;
    using Logging;

    public class DomainEventAggregator : IDomainEventAggregator
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

        private static readonly ILogger Logger = LogManager.Create<DomainEventAggregator>();
        private readonly IDomainEventHandlerProvider _domainEventHandlerProvider;
        private readonly ConcurrentQueue<HandleAction> _handleActions = new ConcurrentQueue<HandleAction>();

        public DomainEventAggregator(IDomainEventHandlerProvider domainEventHandlerProvider)
        {
            _domainEventHandlerProvider = domainEventHandlerProvider;
        }

        /// <summary>
        /// Publish a domain event that is handled by all handlers synchronously in the same scope/transaction.
        /// Possible exceptions are not caught, so that your action might fail due to a failing event handler.
        /// </summary>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <param name="domainEvent"></param>
        public void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            foreach (var injectedHandler in _domainEventHandlerProvider.GetAllEventHandlers<TDomainEvent>())
            {
                HandleAction handleAction = new HandleAction (
                                                            typeof(TDomainEvent).Name,
                                                            injectedHandler.GetType().Name,
                                                            () => injectedHandler.HandleAsync(domainEvent));

                _handleActions.Enqueue(handleAction);
                Logger.Debug($"Invocation of {injectedHandler.GetType().Name} for domain event {typeof(TDomainEvent).Name} registered. It will be executed on completion of unit of work");
            }
        }

        public async Task RaiseEvents()
        {
            while (_handleActions.TryDequeue(out var handleAction))
            {
                try
                {
                    await handleAction.AsyncAction.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Handling of {handleAction.DomainEventName} by {handleAction.HandlerTypeName} failed.");
                    throw;
                }
            }
        }
    }
}
