using System.Linq;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.MultiTenancy
{
    public class MultiTenancyModule<TCurrentTenantIdSelector> : IModule 
        where TCurrentTenantIdSelector : class, ICurrentTenantIdSelector
    
    {
        private readonly bool _isPersistentApplication;

        public MultiTenancyModule(bool isPersistentApplication)
        {
            _isPersistentApplication = isPersistentApplication;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTenantIdSelector, TCurrentTenantIdSelector>());
            
            compositionRoot.RegisterDecorator(
                ServiceDescriptor.Scoped<IOperation, TenantOperationDecorator>());
            
            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTHolder<TenantId>, CurrentTenantIdHolder>());

            if (_isPersistentApplication)
            {
                compositionRoot.RegisterDecorator(
                    ServiceDescriptor.Scoped(typeof(IQueryable<>), typeof(TenantFilteredQueryable<>)));
                
                // compositionRoot.Register(
                //     ServiceDescriptor.Scoped(typeof(ITenantFilterExpressionFactory<>),typeof(TTenantFilterExpressionFactory<>)));
            }
        }
    }
}