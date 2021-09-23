using System;
using System.Diagnostics;
using System.Reflection;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class TypedSubscription : ISubscription
    {
        private static readonly ILogger Logger = LogManager.Create<TypedSubscription>();
        private readonly Type _handlerType;
        private readonly Type _eventType;

        public TypedSubscription(Type handlerType, Type eventType)
        {
            _handlerType = handlerType;
            _eventType = eventType;
        }

        public void Process(IInstanceProvider instanceProvider, EventProcessingContext context)
        {
            IIntegrationEvent integrationEvent = context.GetTypedEvent(_eventType);
            MethodInfo handleMethod = _handlerType.GetRuntimeMethod("Handle", new[] { _eventType });
            Debug.Assert(
                handleMethod != null,
                $"No method with signature `Handle({_eventType.Name} event)` found on {_handlerType.Name}");

            Logger.Info($"Getting subscribed handler instance of type {_handlerType.Name}");
            object handlerInstance = instanceProvider.GetInstance(_handlerType);

            using (Logger.InfoDuration($"Invoking subscribed handler {_handlerType.GetDetailedTypeName()}"))
            {
                handleMethod.Invoke(handlerInstance, new object[] { integrationEvent });
            }
        }

        public bool Matches(object handler)
        {
            return (Type)handler == _handlerType;
        }
    }
}
