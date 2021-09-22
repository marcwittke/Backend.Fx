﻿using System.Collections.Concurrent;
using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface IMessageBusScope
    {
        /// <summary>
        /// Enqueue an event to be raised later.
        /// Intention is to let events bubble up after an operation has terminated
        /// </summary>
        /// <param name="integrationEvent"></param>
        void Publish(IIntegrationEvent integrationEvent);

        Task RaiseEvents();
    }


    public class MessageBusScope : IMessageBusScope
    {
        private readonly ICurrentTHolder<Correlation> _correlationHolder;

        private readonly ConcurrentQueue<IIntegrationEvent> _integrationEvents
            = new ConcurrentQueue<IIntegrationEvent>();

        private readonly IMessageBus _messageBus;

        public MessageBusScope(IMessageBus messageBus, ICurrentTHolder<Correlation> correlationHolder)
        {
            _messageBus = messageBus;
            _correlationHolder = correlationHolder;
        }

        void IMessageBusScope.Publish(IIntegrationEvent integrationEvent)
        {
            ((IntegrationEvent)integrationEvent).SetCorrelationId(_correlationHolder.Current.Id);
            _integrationEvents.Enqueue(integrationEvent);
        }

        public async Task RaiseEvents()
        {
            while (_integrationEvents.TryDequeue(out var integrationEvent))
            {
                await _messageBus.Publish(integrationEvent);
            }
        }
    }
}
