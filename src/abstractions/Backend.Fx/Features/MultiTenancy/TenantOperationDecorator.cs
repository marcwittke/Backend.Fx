using System.Threading.Tasks;
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
        public async Task BeginAsync(IServiceScope serviceScope)
        {
            var currentTenantIdSelector = serviceScope.ServiceProvider.GetRequiredService<ICurrentTenantIdSelector>();
            TenantId currentTenantId = currentTenantIdSelector.GetCurrentTenantId();
            var tenantIdHolder = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<TenantId>>();
            tenantIdHolder.ReplaceCurrent(currentTenantId);
            await _operation.BeginAsync(serviceScope).ConfigureAwait(false);
        }

        public Task CompleteAsync()
        {
            return _operation.CompleteAsync();
        }

        public Task CancelAsync()
        {
            return _operation.CancelAsync();
        }
    }
}