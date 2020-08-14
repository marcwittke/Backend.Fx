using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.Persistence
{
    public class FlushOperationDecorator : IOperation
    {
        private readonly IOperation _operationImplementation;
        private readonly ICanFlush _canFlush;

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
            _canFlush.Flush();
            _operationImplementation.Complete();
        }

        public void Cancel()
        {
            _operationImplementation.Cancel();
        }
    }
}