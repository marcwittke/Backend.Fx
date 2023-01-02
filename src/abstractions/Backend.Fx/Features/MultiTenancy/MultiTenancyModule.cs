using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.MultiTenancy
{
    internal class MultiTenancyModule<TCurrentTenantIdSelector> : IModule
        where TCurrentTenantIdSelector : class, ICurrentTenantIdSelector
    {
        private readonly ITenantEnumerator _tenantEnumerator;
        private readonly ITenantWideMutexManager _tenantWideMutexManager;

        public MultiTenancyModule(ITenantEnumerator tenantEnumerator, ITenantWideMutexManager tenantWideMutexManager)
        {
            _tenantEnumerator = tenantEnumerator;
            _tenantWideMutexManager = tenantWideMutexManager;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(
                ServiceDescriptor.Singleton(_tenantEnumerator));
            
            compositionRoot.Register(
                ServiceDescriptor.Singleton(_tenantWideMutexManager));
            
            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTenantIdSelector, TCurrentTenantIdSelector>());

            compositionRoot.RegisterDecorator(
                ServiceDescriptor.Scoped<IOperation, TenantOperationDecorator>());

            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTHolder<TenantId>, CurrentTenantIdHolder>());
        }
    }
}