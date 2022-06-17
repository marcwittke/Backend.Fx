using Backend.Fx.Extensions;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    [UsedImplicitly]
    public class RaiseIntegrationEventsOperationDecorator : IOperation
    {
        private readonly IMessageBusScope _messageBusScope;
        private readonly IOperation _operation;

        public RaiseIntegrationEventsOperationDecorator(
            IMessageBusScope messageBusScope,
            IOperation operation)
        {
            _messageBusScope = messageBusScope;
            _operation = operation;
        }

        public void Begin()
        {
            _operation.Begin();
        }

        public void Complete()
        {
            _operation.Complete();
            AsyncHelper.RunSync(() => _messageBusScope.RaiseEvents());
        }

        public void Cancel()
        {
            _operation.Cancel();
        }
    }
}