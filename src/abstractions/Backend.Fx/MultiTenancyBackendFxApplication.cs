using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Features;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Features.MultiTenancy.InProc;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx
{
    [PublicAPI]
    public class MultiTenancyBackendFxApplication<TCurrentTenantIdSelector> : BackendFxApplication where TCurrentTenantIdSelector : class, ICurrentTenantIdSelector
    {
        private static readonly ILogger Logger = Log.Create<MultiTenancyBackendFxApplication<TCurrentTenantIdSelector>>();
        
        public MultiTenancyBackendFxApplication(
            ICompositionRoot compositionRoot,
            ITenantEnumerator tenantEnumerator,
            params Assembly[] assemblies)
            : this(compositionRoot, new ExceptionLogger(Logger), tenantEnumerator, new InProcTenantWideMutexManager(), assemblies)
        {
        }
        
        public MultiTenancyBackendFxApplication(
            ICompositionRoot compositionRoot,
            IExceptionLogger exceptionLogger,
            ITenantEnumerator tenantEnumerator,
            params Assembly[] assemblies)
            : this(compositionRoot, exceptionLogger, tenantEnumerator, new InProcTenantWideMutexManager(), assemblies)
        {
        }

        public MultiTenancyBackendFxApplication(
            ICompositionRoot compositionRoot,
            IExceptionLogger exceptionLogger,
            ITenantEnumerator tenantEnumerator,
            ITenantWideMutexManager tenantWideMutexManager,
            params Assembly[] assemblies)
            : base(compositionRoot, exceptionLogger, assemblies)
        {
            CompositionRoot.RegisterModules(new MultiTenancyModule<TCurrentTenantIdSelector>(tenantEnumerator, tenantWideMutexManager));
        }

        public override void EnableFeature(Feature feature)
        {
            base.EnableFeature(feature);
            if (feature is IMultiTenancyFeature multiTenancyFeature)
            {
                multiTenancyFeature.EnableMultiTenancyServices(this);
            }
        }
    }
}