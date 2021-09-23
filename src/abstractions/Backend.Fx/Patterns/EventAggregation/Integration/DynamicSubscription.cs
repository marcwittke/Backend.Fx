using System;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class DynamicSubscription : ISubscription
    {
        private static readonly ILogger Logger = LogManager.Create<DynamicSubscription>();
        private readonly Type _handlerType;

        public DynamicSubscription(Type handlerType)
        {
            _handlerType = handlerType;
        }

        public void Process(IInstanceProvider instanceProvider, EventProcessingContext context)
        {
            Logger.Info($"Getting subscribed handler instance of type {_handlerType.Name}");
            object handlerInstance = instanceProvider.GetInstance(_handlerType);
            using (Logger.InfoDuration($"Invoking subscribed handler {_handlerType.GetDetailedTypeName()}"))
            {
                ((IIntegrationMessageHandler)handlerInstance).Handle(context.DynamicEvent);
            }
        }

        public bool Matches(object handler)
        {
            return (Type)handler == _handlerType;
        }
    }
}
