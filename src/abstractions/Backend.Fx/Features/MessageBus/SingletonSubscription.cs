using System;
using Backend.Fx.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Features.MessageBus
{
    public class SingletonSubscription<TEvent> : ISubscription where TEvent : IIntegrationEvent
    {
        private static readonly ILogger Logger = Log.Create<SingletonSubscription<TEvent>>();
        private readonly IIntegrationMessageHandler<TEvent> _handler;

        public SingletonSubscription(IIntegrationMessageHandler<TEvent> handler)
        {
            _handler = handler;
        }

        public void Process(IServiceProvider serviceProvider, EventProcessingContext context)
        {
            using (Logger.LogInformationDuration($"Invoking subscribed handler {_handler.GetType().Name}"))
            {
                _handler.Handle((TEvent) context.GetTypedEvent(typeof(TEvent)));
            }
        }

        public bool Matches(object handler)
        {
            return _handler == handler;
        }
    }
}