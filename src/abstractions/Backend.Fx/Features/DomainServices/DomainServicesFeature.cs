using System.Reflection;
using JetBrains.Annotations;

namespace Backend.Fx.Features.DomainServices
{
    public static class DomainServicesFeature
    {
        /// <summary>
        /// The feature "Domain Services" makes sure that all implementations of <see cref="IDomainService"/> and
        /// <see cref="IApplicationService"/> are injected as scoped instances.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="assemblies"></param>
        [PublicAPI]
        public static void AddDomainServices(this IBackendFxApplication application)
            => application.CompositionRoot.RegisterModules(new DomainServicesModule(application.Assemblies));
    }
}