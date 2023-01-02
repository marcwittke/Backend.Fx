using JetBrains.Annotations;

namespace Backend.Fx.Features.Jobs
{
    /// <summary>
    /// The feature "Jobs" makes sure, that all implementations of <see cref="IJob"/> are injected as scoped instances.
    /// </summary>
    [PublicAPI]
    public class JobsFeature : Feature, IMultiTenancyFeature
    {
        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new JobsModule(application.Assemblies));
        }

        public void EnableMultiTenancyServices(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new MultiTenancyJobsModule());
        }
    }
}