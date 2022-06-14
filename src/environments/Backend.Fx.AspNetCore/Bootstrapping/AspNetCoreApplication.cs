using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.AspNetCore.Bootstrapping
{
    public class AspNetCoreApplication : BackendFxApplicationDecorator
    {
        public AspNetCoreApplication(IBackendFxApplication application) : base(application)
        {
            application.CompositionRoot.RegisterModules(new AspNetCoreModule(Assemblies));
        }
    }
}