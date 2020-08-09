using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.Persistence
{
    public class ReadonlyDbTransactionOperationDecorator : IOperation
    {
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
            _operationImplementation.Cancel();
        }

        public void Cancel()
        {
            _operationImplementation.Cancel();
        }
    }
}