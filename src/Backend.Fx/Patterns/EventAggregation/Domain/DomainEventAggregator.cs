namespace Backend.Fx.Patterns.EventAggregation.Domain
{
    using System;
    using Logging;

    public class DomainEventAggregator : IDomainEventAggregator
    {
        private static readonly ILogger Logger = LogManager.Create<DomainEventAggregator>();
        private readonly IDomainEventHandlerProvider domainEventHandlerProvider;

        public DomainEventAggregator(IDomainEventHandlerProvider domainEventHandlerProvider)
        {
            this.domainEventHandlerProvider = domainEventHandlerProvider;
        }

        /// <summary>
        /// Publish a domain event that is handled by all handlers synchronously in the same scope/transaction.
        /// Possible exceptions are not caught, so that your action might fail due to a failing evennt handler.
        /// </summary>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <param name="domainEvent"></param>
        public void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            foreach (var injectedHandler in domainEventHandlerProvider.GetAllEventHandlers<TDomainEvent>())
            {
                try
                {
                    injectedHandler.Handle(domainEvent);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Handling of {typeof(TDomainEvent).Name} by {injectedHandler.GetType().Name} failed.");
                    throw;
                }
            }
        }
    }
}
