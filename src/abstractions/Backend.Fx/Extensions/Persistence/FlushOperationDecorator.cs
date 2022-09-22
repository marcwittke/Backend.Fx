using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Extensions.Persistence
{
    public class FlushOperationDecorator : IOperation
    {
        private static readonly ILogger Logger = Log.Create<FlushOperationDecorator>();
        private readonly IOperation _operationImplementation;
        private readonly ICanFlush _canFlush;

        public FlushOperationDecorator(ICanFlush canFlush, IOperation operationImplementation)
        {
            _operationImplementation = operationImplementation;
            _canFlush = canFlush;
        }

        public void Begin(IServiceScope serviceScope)
        {
            _operationImplementation.Begin(serviceScope);
        }

        public void Complete()
        {
            Logger.LogDebug("Flushing before completion of operation");
            _canFlush.Flush();
            _operationImplementation.Complete();
        }

        public void Cancel()
        {
            _operationImplementation.Cancel();
        }
    }
}