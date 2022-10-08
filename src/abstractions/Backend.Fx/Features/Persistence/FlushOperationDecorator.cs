using System.Threading;
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

        public Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
        {
            return _operationImplementation.BeginAsync(serviceScope, cancellationToken);
        }

        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogDebug("Flushing before completion of operation");
            _canFlush.Flush();
            await _operationImplementation.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task CancelAsync(CancellationToken cancellationToken = default)
        {
            return _operationImplementation.CancelAsync(cancellationToken);
        }
    }
}