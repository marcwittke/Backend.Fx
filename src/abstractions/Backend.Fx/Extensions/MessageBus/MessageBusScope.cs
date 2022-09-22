using System.Collections.Concurrent;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Util;

namespace Backend.Fx.Extensions.MessageBus
{
    public interface IMessageBusScope
    {
        /// <summary>
        /// Enqueue an event to be raised later. 
        /// Intention is to let events bubble up after an operation has terminated
        /// </summary>
        /// <param name="integrationEvent"></param>
        void Publish(IIntegrationEvent integrationEvent);

        Task RaiseEventsAsync();
    }

    internal class MessageBusScope : IMessageBusScope
    {
        private readonly ConcurrentQueue<IIntegrationEvent> _integrationEvents = new ConcurrentQueue<IIntegrationEvent>();
        private readonly MessageBus _messageBus;
        private readonly ICurrentTHolder<Correlation> _correlationHolder;
        
        public MessageBusScope(
            MessageBus messageBus,
            ICurrentTHolder<Correlation> correlationHolder)
        {
            _messageBus = messageBus;
            _correlationHolder = correlationHolder;
        }

        void IMessageBusScope.Publish(IIntegrationEvent integrationEvent)
        {
            ((IntegrationEvent) integrationEvent).CorrelationId = _correlationHolder.Current.Id;
            _integrationEvents.Enqueue(integrationEvent);
        }

        public async Task RaiseEventsAsync()
        {
            while (_integrationEvents.TryDequeue(out IIntegrationEvent integrationEvent))
            {
                await _messageBus.PublishAsync(integrationEvent).ConfigureAwait(false);
            }
        }
    }
}