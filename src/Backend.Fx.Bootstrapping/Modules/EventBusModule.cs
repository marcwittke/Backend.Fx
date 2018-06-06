namespace Backend.Fx.Bootstrapping.Modules
{
    using System.Linq;
    using System.Reflection;
    using Logging;
    using Patterns.EventAggregation.Integration;
    using SimpleInjector;

    public class EventBusModule : SimpleInjectorModule
    {
        private static readonly ILogger Logger = LogManager.Create<EventBusModule>();

        private readonly IEventBus eventBus;
        private readonly Assembly[] assemblies;
        private readonly string assembliesForLogging;

        public EventBusModule(IEventBus eventBus, params Assembly[] assemblies)
        {
            this.eventBus = eventBus;
            this.assemblies = assemblies;
            assembliesForLogging = string.Join(",", assemblies.Select(ass => ass.GetName().Name));
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterInstance(eventBus);

            Logger.Debug($"Registering generic integration event handlers from {assembliesForLogging}");
            container.Register(typeof(IIntegrationEventHandler<>), assemblies);

            Logger.Debug($"Registering dynamic integration event handlers from {assembliesForLogging}");
            foreach (var dynamicHandlerType in container.GetTypesToRegister(typeof(IIntegrationEventHandler), assemblies))
            {
                Logger.Debug($"Registering dynamic integration event {dynamicHandlerType.Name} as transient");
                container.Register(dynamicHandlerType);
            }
        }
    }
}
