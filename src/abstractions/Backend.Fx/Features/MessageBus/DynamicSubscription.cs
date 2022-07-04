using System;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;


namespace Backend.Fx.Features.MessageBus
{
    public class DynamicSubscription : ISubscription
    {
        private static readonly ILogger Logger = Log.Create<DynamicSubscription>();
        private readonly Type _handlerType;

        public DynamicSubscription(Type handlerType)
        {
            _handlerType = handlerType;
        }

        public void Process(IServiceProvider serviceProvider, EventProcessingContext context)
        {
            Logger.LogInformation("Getting subscribed handler instance of type {HandlerTypeName}", _handlerType.Name);
            object handlerInstance = serviceProvider.GetRequiredService(_handlerType);
            using (Logger.LogInformationDuration($"Invoking subscribed handler {_handlerType.GetDetailedTypeName()}"))
            {
                ((IIntegrationMessageHandler) handlerInstance).Handle(context.DynamicEvent);
            }
        }

        public bool Matches(object handler)
        {
            return (Type) handler == _handlerType;
        }
    }
}