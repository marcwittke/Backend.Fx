using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class SingletonSubscription<TEvent>: ISubscription where TEvent : IIntegrationEvent
    {
        private static readonly ILogger Logger = LogManager.Create<SingletonSubscription<TEvent>>();
        private readonly IIntegrationEventHandler<TEvent> _handler;

        public SingletonSubscription(IIntegrationEventHandler<TEvent> handler)
        {
            _handler = handler;
        }

        public void Process(string eventName, EventProcessingContext context)
        {
            using (Logger.InfoDuration($"Invoking subscribed handler {_handler.GetType().Name}"))
            {
                _handler.Handle((TEvent)context.GetTypedEvent(typeof(TEvent)));
            }
        }

        public bool Matches(object handler)
        {
            return _handler == handler;
        }
    }
}