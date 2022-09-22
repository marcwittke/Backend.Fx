using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.MultiTenancy
{
    [UsedImplicitly]
    public class TenantOperationDecorator : IOperation
    {
        private readonly IOperation _operation;

        public TenantOperationDecorator(IOperation operation)
        {
            _operation = operation;
        }
        public void Begin(IServiceScope serviceScope)
        {
            var currentTenantIdSelector = serviceScope.ServiceProvider.GetRequiredService<ICurrentTenantIdSelector>();
            var currentTenantId = currentTenantIdSelector.GetCurrentTenantId();
            var tenantIdHolder = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<TenantId>>();
            tenantIdHolder.ReplaceCurrent(currentTenantId);
            _operation.Begin(serviceScope);
        }

        public void Complete()
        {
            _operation.Complete();
        }

        public void Cancel()
        {
            _operation.Cancel();
        }
    }
}