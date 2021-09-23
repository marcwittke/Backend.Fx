using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.Persistence
{
    public class FlushOperationDecorator : IOperation
    {
        private static readonly ILogger Logger = LogManager.Create<FlushOperationDecorator>();
        private readonly ICanFlush _canFlush;
        private readonly IOperation _operationImplementation;

        public FlushOperationDecorator(ICanFlush canFlush, IOperation operationImplementation)
        {
            _operationImplementation = operationImplementation;
            _canFlush = canFlush;
        }

        public void Begin()
        {
            _operationImplementation.Begin();
        }

        public void Complete()
        {
            Logger.Debug("Flushing before completion of operation");
            _canFlush.Flush();
            _operationImplementation.Complete();
        }

        public void Cancel()
        {
            _operationImplementation.Cancel();
        }
    }
}
