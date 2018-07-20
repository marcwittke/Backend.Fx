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
        private readonly ConcurrentQueue<IIntegrationEvent> integrationEvents = new ConcurrentQueue<IIntegrationEvent>();
        private readonly IEventBus eventBus;

        public EventBusScope(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }
        
        void IEventBusScope.Publish(IIntegrationEvent integrationEvent)
        {
            integrationEvents.Enqueue(integrationEvent);
        }

        public async Task RaiseEvents()
        {
            while (integrationEvents.TryDequeue(out var integrationEvent))
            {
                await eventBus.Publish(integrationEvent);
            }
        }
    }
}

