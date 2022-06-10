using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.Jobs
{

    public class JobApplication : BackendFxApplicationDecorator
    {
        public JobApplication(IBackendFxApplication application)
            : base(application)
        {
            application.CompositionRoot.RegisterModules(new JobModule(application.Assemblies));
        }
    }
}