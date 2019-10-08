using System;
using System.Threading.Tasks;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface IEventBus : IDisposable
    {
        void Connect();

        /// <summary>
        /// Directly publishes an event on the event bus without delay.
        /// In most cases you want to publish an event when the cause is considered as safely done, e.g. when the 
        /// wrapping transaction is committed. Use <see cref="IEventBusScope"/> to let the framework raise all events
        /// after committing the unit of work.
        /// </summary>
        /// <param name="integrationEvent"></param>
        /// <returns></returns>
        void Publish(IIntegrationEvent integrationEvent);

        /// <summary>
        /// Subscribes to an integration event with a dynamic event handler
        /// </summary>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <param name="eventName">Th event name to subscribe to. (Should be Type.FullName to avoid namespace collisions)</param>
        void Subscribe<THandler>(string eventName)
            where THandler : IIntegrationEventHandler;

        /// <summary>
        /// Subscribes to an integration event with a generically typed event handler
        /// </summary>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <typeparam name="TEvent">The event type to subscribe to</typeparam>
        void Subscribe<THandler, TEvent>()
            where THandler : IIntegrationEventHandler<TEvent>
            where TEvent : IIntegrationEvent;

        /// <summary>
        /// Subscribes to an integration event with a singleton instance event handler
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to</typeparam>
        void Subscribe<TEvent>(IIntegrationEventHandler<TEvent> handler)
            where TEvent : IIntegrationEvent;

        void Unsubscribe<THandler>(string eventName)
            where THandler : IIntegrationEventHandler;

        void Unsubscribe<THandler, TEvent>()
            where THandler : IIntegrationEventHandler<TEvent>
            where TEvent : IIntegrationEvent;

        void Unsubscribe<TEvent>(IIntegrationEventHandler<TEvent> handler)
            where TEvent : IIntegrationEvent;
    }
}