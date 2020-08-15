using System;
using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface IMessageBus : IDisposable
    {
        void Connect();

        /// <summary>
        /// Directly publishes an event on the message bus without delay.
        /// In most cases you want to publish an event when the cause is considered as safely done, e.g. when the 
        /// wrapping transaction is committed. Use <see cref="IMessageBusScope"/> to let the framework raise all events
        /// after committing the unit of work.
        /// </summary>
        /// <param name="integrationEvent"></param>
        /// <returns></returns>
        Task Publish(IIntegrationEvent integrationEvent);

        /// <summary>
        /// Subscribes to an integration event with a dynamic event handler
        /// </summary>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <param name="eventName">Th event name to subscribe to. (Should be Type.FullName to avoid namespace collisions)</param>
        void Subscribe<THandler>(string eventName)
            where THandler : IIntegrationMessageHandler;

        /// <summary>
        /// Subscribes to an integration event with a generically typed event handler
        /// </summary>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <typeparam name="TEvent">The event type to subscribe to</typeparam>
        void Subscribe<THandler, TEvent>()
            where THandler : IIntegrationMessageHandler<TEvent>
            where TEvent : IIntegrationEvent;

        /// <summary>
        /// Subscribes to an integration event with a singleton instance event handler
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to</typeparam>
        void Subscribe<TEvent>(IIntegrationMessageHandler<TEvent> handler)
            where TEvent : IIntegrationEvent;

        void Unsubscribe<THandler>(string eventName)
            where THandler : IIntegrationMessageHandler;

        void Unsubscribe<THandler, TEvent>()
            where THandler : IIntegrationMessageHandler<TEvent>
            where TEvent : IIntegrationEvent;

        void Unsubscribe<TEvent>(IIntegrationMessageHandler<TEvent> handler)
            where TEvent : IIntegrationEvent;

        void IntegrateApplication(IBackendFxApplicationInvoker invoker);
    }
}