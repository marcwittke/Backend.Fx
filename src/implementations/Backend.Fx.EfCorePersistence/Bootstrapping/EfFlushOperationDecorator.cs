using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public class EfFlushOperationDecorator : IOperation
    {
        private readonly IOperation _operationImplementation;
        private readonly ICanFlush _canFlush;

        public EfFlushOperationDecorator(IOperation operationImplementation, ICanFlush canFlush)
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