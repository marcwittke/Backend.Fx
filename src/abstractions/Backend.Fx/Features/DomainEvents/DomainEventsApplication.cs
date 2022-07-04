using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Features.DomainEvents
{
    public class DomainEventsApplication : BackendFxApplicationDecorator
    {
        public DomainEventsApplication(IBackendFxApplication application) : base(application)
        {
            application.CompositionRoot.RegisterModules(new DomainEventsModule(application.Assemblies));
        }
    }
}