using System.Linq;
using System.Reflection;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    public class EventBusModule : SimpleInjectorModule
    {
        private static readonly ILogger Logger = LogManager.Create<EventBusModule>();

        private readonly IEventBus _eventBus;
        private readonly Assembly[] _assemblies;
        private readonly string _assembliesForLogging;

        public EventBusModule(IEventBus eventBus, params Assembly[] assemblies)
        {
            this._eventBus = eventBus;
            this._assemblies = assemblies;
            _assembliesForLogging = string.Join(",", assemblies.Select(ass => ass.GetName().Name));
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterInstance(_eventBus);

            Logger.Debug($"Registering generic integration event handlers from {_assembliesForLogging}");
            container.Register(typeof(IIntegrationEventHandler<>), _assemblies);

            Logger.Debug($"Registering dynamic integration event handlers from {_assembliesForLogging}");
            foreach (var dynamicHandlerType in container.GetTypesToRegister(typeof(IIntegrationEventHandler), _assemblies))
            {
                Logger.Debug($"Registering dynamic integration event {dynamicHandlerType.Name} as transient");
                container.Register(dynamicHandlerType);
            }
        }
    }
}
