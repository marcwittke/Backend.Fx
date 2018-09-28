namespace Backend.Fx.Patterns.EventAggregation.Domain
{
    using System;
    using System.Collections.Concurrent;
    using Logging;

    public class DomainEventAggregator : IDomainEventAggregator
    {
        private static readonly ILogger Logger = LogManager.Create<DomainEventAggregator>();
        private readonly IDomainEventHandlerProvider _domainEventHandlerProvider;
        private readonly ConcurrentQueue<(string, string, Action)> _handleActions = new ConcurrentQueue<(string, string, Action)>();

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
                (string, string, Action) handleAction = (
                                                            typeof(TDomainEvent).Name,
                                                            injectedHandler.GetType().Name,
                                                            () => injectedHandler.Handle(domainEvent));

                _handleActions.Enqueue(handleAction);
                Logger.Debug($"Invocation of {injectedHandler.GetType().Name} for domain event {typeof(TDomainEvent).Name} registered. It will be executed on completion of unit of work");
            }
        }

        public void RaiseEvents()
        {
            while (_handleActions.TryDequeue(out var handleAction))
            {
                try
                {
                    handleAction.Item3.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Handling of {handleAction.Item1} by {handleAction.Item2} failed.");
                    throw;
                }
            }
        }
    }
}
