using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.Jobs
{
    public abstract class ApplicationWithJobs : BackendFxApplicationDecorator
    {
        protected ApplicationWithJobs(IBackendFxApplication application)
            : base(application)
        {
            application.CompositionRoot.RegisterModules(new JobModule(application.Assemblies));
        }
    }
}