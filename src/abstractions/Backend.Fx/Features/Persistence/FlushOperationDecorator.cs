using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Features.Persistence
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

        public void Begin()
        {
            _operationImplementation.Begin();
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