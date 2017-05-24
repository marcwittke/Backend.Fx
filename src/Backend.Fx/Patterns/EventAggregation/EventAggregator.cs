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
        }

        public IEnumerable<Task> PublishIntegrationEvent<TIntegrationEvent>(TIntegrationEvent integrationEvent) where TIntegrationEvent : IIntegrationEvent
        {
            foreach (var subscribedHandler in subscribedEventHandlers.OfType<Action<TIntegrationEvent>>())
            {
                yield return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        subscribedHandler.Invoke(integrationEvent);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Handling of integration event {typeof(TIntegrationEvent).Name} by a subscribed handler failed.");
                    }
                });
            }
        }

        public void SubscribeToIntegrationEvent<TIntegrationEvent>(Action<TIntegrationEvent> handler) where TIntegrationEvent : IIntegrationEvent
        {
            subscribedEventHandlers.Add(handler);
        }
    }
}
