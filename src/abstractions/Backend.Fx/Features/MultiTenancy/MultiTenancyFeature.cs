using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Extensions.Persistence;
using Backend.Fx.Util;
using JetBrains.Annotations;

namespace Backend.Fx.Features.MultiTenancy
{
    /// <summary>
    /// The feature "Multi Tenancy" makes you provide an implementation of <see cref="ICurrentTenantIdSelector"/> and
    /// takes care of asking it for the current tenant id and setting it in the <see cref="ICurrentTHolder{TenantId}"/>
    /// on every operation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicAPI]
    public static class MultiTenancyFeature
    {
        public static void EnableMultiTenancy<T> (this IBackendFxApplication application)
            where T : class, ICurrentTenantIdSelector
        {
            application.CompositionRoot.RegisterModules(
                new MultiTenancyModule<T>(application.As<PersistentApplication>() != null));

            ((BackendFxApplication)application).IsMultiTenancyApplication = true;
        }
    }
}