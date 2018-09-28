namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using BuildingBlocks;

    public interface IEventBusScope : IDomainService
    { 
        /// <summary>
        /// Enqueues an event to be raised later. 
        /// Intention is to let events bubble up after an operation has terminated, e.g. when a wrapping
        /// unit of work has completed.
        /// </summary>
        /// <param name="integrationEvent"></param>
        void Publish(IIntegrationEvent integrationEvent);
        Task RaiseEvents();
    }

    public class EventBusScope : IEventBusScope
    {
        private readonly ConcurrentQueue<IIntegrationEvent> _integrationEvents = new ConcurrentQueue<IIntegrationEvent>();
        private readonly IEventBus _eventBus;

        public EventBusScope(IEventBus eventBus)
        {
            this._eventBus = eventBus;
        }
        
        void IEventBusScope.Publish(IIntegrationEvent integrationEvent)
        {
            _integrationEvents.Enqueue(integrationEvent);
        }

        public async Task RaiseEvents()
        {
            while (_integrationEvents.TryDequeue(out var integrationEvent))
            {
                await _eventBus.Publish(integrationEvent);
            }
        }
    }
}

