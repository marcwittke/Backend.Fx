using JetBrains.Annotations;

namespace Backend.Fx.Features.DomainServices
{
    /// <summary>
    /// The feature "Domain Services" makes sure that all implementations of <see cref="IDomainService"/> and
    /// <see cref="IApplicationService"/> are injected as scoped instances.
    /// </summary>
    [PublicAPI]
    public class DomainServicesFeature : Feature
    {
        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new DomainServicesModule(application.Assemblies));
        }
    }
}