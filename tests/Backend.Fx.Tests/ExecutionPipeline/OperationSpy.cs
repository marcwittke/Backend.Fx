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

    public async Task BeginAsync(IServiceScope serviceScope)
    {
        await _operationSpy.BeginAsync(serviceScope);
        await _operation.BeginAsync(serviceScope);
    }

    public async Task CompleteAsync()
    {
        await _operationSpy.CompleteAsync();
        await _operation.CompleteAsync();
    }

    public async Task CancelAsync()
    {
        await _operationSpy.CancelAsync();
        await _operation.CancelAsync();
    }
}