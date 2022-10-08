using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Tests.ExecutionPipeline;

public interface IOperationSpy : IOperation
{
}

public class OperationSpy : IOperation
{
    private readonly IOperationSpy _operationSpy;
    private readonly IOperation _operation;

    public OperationSpy(IOperationSpy operationSpy, IOperation operation)
    {
        _operationSpy = operationSpy;
        _operation = operation;
    }

    public async Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
    {
        await _operationSpy.BeginAsync(serviceScope);
        await _operation.BeginAsync(serviceScope);
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await _operationSpy.CompleteAsync(cancellationToken);
        await _operation.CompleteAsync(cancellationToken);
    }

    public async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        await _operationSpy.CancelAsync(cancellationToken);
        await _operation.CancelAsync(cancellationToken);
    }
}