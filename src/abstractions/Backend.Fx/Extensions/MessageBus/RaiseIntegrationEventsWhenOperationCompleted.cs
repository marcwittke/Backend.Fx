using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Extensions.MessageBus
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

        public void Begin(IServiceScope serviceScope)
        {
            _operation.Begin(serviceScope);
        }

        public void Complete()
        {
            _operation.Complete();
            AsyncHelper.RunSync(() => _messageBusScope.RaiseEventsAsync());
        }

        public void Cancel()
        {
            _operation.Cancel();
        }
    }
}