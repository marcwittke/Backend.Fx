namespace Backend.Fx.Patterns.EventAggregation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Logging;

    public class EventAggregator : IEventAggregator
    {
        private static readonly ILogger Logger = LogManager.Create<EventAggregator>();
        private readonly IEventHandlerProvider eventHandlerProvider;
        private readonly ISet<Delegate> subscribedEventHandlers = new HashSet<Delegate>();

        public EventAggregator(IEventHandlerProvider eventHandlerProvider)
        {
            this.eventHandlerProvider = eventHandlerProvider;
        }

        /// <summary>
        /// Publish a domain event that is handled by all handlers synchronously in the same scope/transaction.
        /// Possible exceptions are not caught, so that your action might fail due to a failing evennt handler.
        /// </summary>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <param name="domainEvent"></param>
        public void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            foreach (var injectedHandler in eventHandlerProvider.GetAllEventHandlers<TDomainEvent>())
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

            foreach (var subscribedHandler in subscribedEventHandlers.OfType<Action<TDomainEvent>>().ToArray())
            {
                try
                {
                    subscribedHandler.Invoke(domainEvent);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Handling of {typeof(TDomainEvent).Name} by a subscribed handler failed.");
                    throw;
                }
            }
        }

        /// <summary>
        /// Publish an integration event that is handled asynchronously in a specific scope/transaction.
        /// Possible exceptions are caught, logged and swallowed. Manual recovery is required.
        /// </summary>
        /// <typeparam name="TIntegrationEvent"></typeparam>
        /// <param name="integrationEvent"></param>
        /// <returns></returns>
        public Task PublishIntegrationEvent<TIntegrationEvent>(TIntegrationEvent integrationEvent) where TIntegrationEvent : IIntegrationEvent
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var subscribedHandler in subscribedEventHandlers.OfType<Action<TIntegrationEvent>>().ToArray())
                {
                    try
                    {
                        subscribedHandler.Invoke(integrationEvent);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Handling of integration event {typeof(TIntegrationEvent).Name} by a subscribed handler failed.");
                    }
                }
            });
        }

        /// <summary>
        /// Register a delegate that should be called asynchronously in a specific scope/transaction when the specific integration event is published.
        /// </summary>
        /// <typeparam name="TIntegrationEvent"></typeparam>
        /// <param name="handler"></param>
        public void SubscribeToIntegrationEvent<TIntegrationEvent>(Action<TIntegrationEvent> handler) where TIntegrationEvent : IIntegrationEvent
        {
            subscribedEventHandlers.Add(handler);
        }

        /// <summary>
        /// Register a delegate that should be called synchronously in the same scope/transaction where the domain event is published.
        /// </summary>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <param name="handler"></param>
        public void SubscribeToDomainEvent<TDomainEvent>(Action<TDomainEvent> handler) where TDomainEvent : IDomainEvent
        {
            subscribedEventHandlers.Add(handler);
        }
    }
}
