namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System.Collections.Concurrent;
    
    public interface IEventBusScope
    { 
        /// <summary>
        /// Enqueue an event to be raised later. 
        /// Intention is to let events bubble up after an operation has terminated, e.g. when a wrapping
        /// unit of work has completed.
        /// </summary>
        /// <param name="integrationEvent"></param>
        void Publish(IIntegrationEvent integrationEvent);
        void RaiseEvents();
    }

    public class EventBusScope : IEventBusScope
    {
        private readonly ConcurrentQueue<IIntegrationEvent> _integrationEvents = new ConcurrentQueue<IIntegrationEvent>();
        private readonly IEventBus _eventBus;

        public EventBusScope(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        
        void IEventBusScope.Publish(IIntegrationEvent integrationEvent)
        {
            _integrationEvents.Enqueue(integrationEvent);
        }

        public void RaiseEvents()
        {
            while (_integrationEvents.TryDequeue(out var integrationEvent))
            {
                _eventBus.Publish(integrationEvent);
            }
        }
    }
}

