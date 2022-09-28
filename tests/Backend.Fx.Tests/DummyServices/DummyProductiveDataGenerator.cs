using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.DataGeneration;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Tests.DummyServices;

[UsedImplicitly]
public class DummyProductiveDataGenerator : DataGenerator, IProductiveDataGenerator
{
    private readonly DummyProductiveDataGeneratorSpy _spy;
    private static readonly ILogger Logger = Log.Create<DummyProductiveDataGenerator>();
    
    public DummyProductiveDataGenerator(DummyProductiveDataGeneratorSpy spy)
    {
        _spy = spy;
    }
    
    public override int Priority => 1;
    
    protected override Task GenerateCoreAsync(CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebug("Invocation {Invocations} of DummyProductiveDataGenerator happens",
                Interlocked.Increment(ref _spy.InvocationCount));
        }
        else
        {
            Interlocked.Increment(ref _spy.InvocationCount);
        }

        return Task.CompletedTask;
    }

    protected override void Initialize()
    { }

    protected override bool ShouldRun() => _spy.ShouldRun;
}


public class DummyProductiveDataGeneratorSpy
{
    public int InvocationCount;
    public bool ShouldRun = true;
}