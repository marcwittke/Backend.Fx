using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        public Task BeginAsync(IServiceScope serviceScope)
        {
            return _operationImplementation.BeginAsync(serviceScope);
        }

        public async Task CompleteAsync()
        {
            Logger.LogDebug("Flushing before completion of operation");
            _canFlush.Flush();
            await _operationImplementation.CompleteAsync().ConfigureAwait(false);
        }

        public Task CancelAsync()
        {
            return _operationImplementation.CancelAsync();
        }
    }
}