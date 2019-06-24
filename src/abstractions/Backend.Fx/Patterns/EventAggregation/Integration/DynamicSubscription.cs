using System;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class DynamicSubscription : ISubscription
    {
        private static readonly ILogger Logger = LogManager.Create<DynamicSubscription>();
        private readonly IBackendFxApplication _application;
        private readonly Type _handlerType;

        public DynamicSubscription(IBackendFxApplication application, Type handlerType)
        {
            _application = application;
            _handlerType = handlerType;
        }

        public void Process(string eventName, EventProcessingContext context)
        {
            Logger.Info($"Getting subscribed handler instance of type {_handlerType.Name}");
            object handlerInstance = _application.CompositionRoot.GetInstance(_handlerType);
            using (Logger.InfoDuration($"Invoking subscribed handler {_handlerType.GetDetailedTypeName()}"))
            {
                ((IIntegrationEventHandler)handlerInstance).Handle(context.DynamicEvent);
            }
        }

        public bool Matches(object handler)
        {
            return (Type) handler == _handlerType;
        }
    }
}