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

        public Task BeginAsync(IServiceScope serviceScope)
        {
            return _operation.BeginAsync(serviceScope);
        }

        public async Task CompleteAsync()
        {
            await _operation.CompleteAsync().ConfigureAwait(false);
            await _messageBusScope.RaiseEventsAsync().ConfigureAwait(false);
        }

        public Task CancelAsync()
        {
            return _operation.CancelAsync();
        }
    }
}