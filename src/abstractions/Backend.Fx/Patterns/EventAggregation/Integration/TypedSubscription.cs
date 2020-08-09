using System;
using System.Diagnostics;
using System.Linq;
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

        public TypedSubscription(Type handlerType)
        {
            _handlerType = handlerType;
        }

        public void Process(IInstanceProvider instanceProvider, EventProcessingContext context)
        {
            Type interfaceType = _handlerType.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IIntegrationMessageHandler<>));
            Type eventType = interfaceType.GetGenericArguments().Single(t => typeof(IIntegrationEvent).IsAssignableFrom(t));
            IIntegrationEvent integrationEvent = context.GetTypedEvent(eventType);
            MethodInfo handleMethod = _handlerType.GetRuntimeMethod("Handle", new[] {eventType});
            Debug.Assert(handleMethod != null, $"No method with signature `Handle({eventType.Name} event)` found on {_handlerType.Name}");

            Logger.Info($"Getting subscribed handler instance of type {_handlerType.Name}");
            object handlerInstance = instanceProvider.GetInstance(_handlerType);

            using (Logger.InfoDuration($"Invoking subscribed handler {_handlerType.GetDetailedTypeName()}"))
            {
                handleMethod.Invoke(handlerInstance, new object[] {integrationEvent});
            }
        }

        public bool Matches(object handler)
        {
            return (Type) handler == _handlerType;
        }
    }
}