using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.MessageBus
{
    [UsedImplicitly]
    internal class RaiseIntegrationEventsWhenOperationCompleted : IOperation
    {
        private readonly IMessageBusScope _messageBusScope;
        private readonly IOperation _operation;

        public RaiseIntegrationEventsWhenOperationCompleted(
            IMessageBusScope messageBusScope,
            IOperation operation)
        {
            _messageBusScope = messageBusScope;
            _operation = operation;
        }

        public Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
        {
            return _operation.BeginAsync(serviceScope, cancellationToken);
        }

        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            await _operation.CompleteAsync(cancellationToken).ConfigureAwait(false);
            await _messageBusScope.RaiseEventsAsync().ConfigureAwait(false);
        }

        public Task CancelAsync(CancellationToken cancellationToken = default)
        {
            return _operation.CancelAsync(cancellationToken);
        }
    }
}