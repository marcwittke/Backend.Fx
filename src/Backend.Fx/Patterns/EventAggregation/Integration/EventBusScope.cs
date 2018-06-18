namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BuildingBlocks;

    public interface IEventBusScope : IDomainService
    { 
        void Publish(IIntegrationEvent integrationEvent);
        Task RaiseEvents();
    }

    public class EventBusScope : IEventBusScope
    {
        private readonly List<IIntegrationEvent> integrationEvents = new List<IIntegrationEvent>();
        private readonly IEventBus eventBus;

        public EventBusScope(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }
        
        void IEventBusScope.Publish(IIntegrationEvent integrationEvent)
        {
            integrationEvents.Add(integrationEvent);
        }

        public async Task RaiseEvents()
        {
            foreach (var integrationEvent in integrationEvents)
            {
                await eventBus.Publish(integrationEvent);
            }
        }
    }
}

