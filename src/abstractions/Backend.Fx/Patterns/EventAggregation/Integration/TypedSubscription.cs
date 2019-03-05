using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public class TypedSubscription : ISubscription
    {
        private static readonly ILogger Logger = LogManager.Create<TypedSubscription>();
        private readonly IBackendFxApplication _application;
        private readonly Type _handlerType;

        public TypedSubscription(IBackendFxApplication application, Type handlerType)
        {
            _application = application;
            _handlerType = handlerType;
        }

        public void Process(string eventName, EventProcessingContext context)
        {
            Type interfaceType = _handlerType.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));
            var eventType = interfaceType.GetGenericArguments().Single(t => typeof(IIntegrationEvent).IsAssignableFrom(t));
            var integrationEvent = context.GetTypedEvent(eventType);
            MethodInfo handleMethod = _handlerType.GetRuntimeMethod("Handle", new[] { eventType });
            Debug.Assert(handleMethod != null, $"No method with signature `Handle({eventName} event)` found on {_handlerType.Name}");

            Logger.Info($"Getting subscribed handler instance of type {_handlerType.Name}");
            object handlerInstance = _application.CompositionRoot.GetInstance(_handlerType);

            using (Logger.InfoDuration($"Invoking subscribed handler {_handlerType.Name}"))
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