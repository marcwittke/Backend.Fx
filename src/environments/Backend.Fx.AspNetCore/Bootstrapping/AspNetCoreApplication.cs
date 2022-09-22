using Backend.Fx.Extensions;

namespace Backend.Fx.AspNetCore.Bootstrapping
{
    public class AspNetCoreApplication : BackendFxApplicationExtension
    {
        public AspNetCoreApplication(IBackendFxApplication application) : base(application)
        {
            application.CompositionRoot.RegisterModules(new AspNetCoreModule(Assemblies));
        }
    }
}