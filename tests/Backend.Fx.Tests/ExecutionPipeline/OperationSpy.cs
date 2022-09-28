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

    public void Begin(IServiceScope serviceScope)
    {
        _operationSpy.Begin(serviceScope);
        _operation.Begin(serviceScope);
    }

    public void Complete()
    {
        _operationSpy.Complete();
        _operation.Complete();
    }

    public void Cancel()
    {
        _operationSpy.Cancel();
        _operation.Cancel();
    }
}