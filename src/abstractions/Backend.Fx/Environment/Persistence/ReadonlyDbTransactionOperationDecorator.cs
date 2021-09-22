using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.Persistence
{
    public class ReadonlyDbTransactionOperationDecorator : IOperation
    {
        private static readonly ILogger Logger = LogManager.Create<ReadonlyDbTransactionOperationDecorator>();
        private readonly IOperation _operationImplementation;

        public ReadonlyDbTransactionOperationDecorator(IOperation operationImplementation)
        {
            _operationImplementation = operationImplementation;
        }

        public void Begin()
        {
            _operationImplementation.Begin();
        }

        public void Complete()
        {
            Logger.Debug("Canceling operation instead of completing it due to classification as readonly operation");
            _operationImplementation.Cancel();
        }

        public void Cancel()
        {
            _operationImplementation.Cancel();
        }
    }
}
