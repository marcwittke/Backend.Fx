﻿using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

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
        private readonly ConcurrentQueue<IIntegrationEvent> _integrationEvents = new ConcurrentQueue<IIntegrationEvent>();
        private readonly IMessageBus _messageBus;
        private readonly ICurrentTHolder<Correlation> _correlationHolder;
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;

        public MessageBusScope(IMessageBus messageBus,
                               ICurrentTHolder<Correlation> correlationHolder,
                               ICurrentTHolder<TenantId> tenantIdHolder)
        {
            _messageBus = messageBus;
            _correlationHolder = correlationHolder;
            _tenantIdHolder = tenantIdHolder;
        }

        void IMessageBusScope.Publish(IIntegrationEvent integrationEvent)
        {
             integrationEvent.CorrelationId = _correlationHolder.Current.Id;
             integrationEvent.TenantId = _tenantIdHolder.Current.Value;
            _integrationEvents.Enqueue(integrationEvent);
        }

        public async Task RaiseEvents()
        {
            while (_integrationEvents.TryDequeue(out IIntegrationEvent integrationEvent))
            {
                await _messageBus.Publish(integrationEvent);
            }
        }
    }
}