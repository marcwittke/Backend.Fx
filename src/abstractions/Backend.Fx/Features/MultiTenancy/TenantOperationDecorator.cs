using System.Threading;
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
        public async Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
        {
            var currentTenantIdSelector = serviceScope.ServiceProvider.GetRequiredService<ICurrentTenantIdSelector>();
            TenantId currentTenantId = currentTenantIdSelector.GetCurrentTenantId();
            var tenantIdHolder = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<TenantId>>();
            tenantIdHolder.ReplaceCurrent(currentTenantId);
            await _operation.BeginAsync(serviceScope, cancellationToken).ConfigureAwait(false);
        }

        public Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            return _operation.CompleteAsync(cancellationToken);
        }

        public Task CancelAsync(CancellationToken cancellationToken = default)
        {
            return _operation.CancelAsync(cancellationToken);
        }
    }
}