using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.DataGeneration;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Tests.DummyServices;

[UsedImplicitly]
public class DummyDemoDataGenerator : DataGenerator, IDemoDataGenerator
{
    private readonly DummyDemoDataGeneratorSpy _spy;
    private readonly ILogger _logger = Log.Create<DummyDemoDataGenerator>();
    private static readonly AsyncLocal<IDemoDataGenerator> Spy = new();

    public DummyDemoDataGenerator(DummyDemoDataGeneratorSpy spy)
    {
        _spy = spy;
    }

    public override int Priority => 1;

    protected override Task GenerateCoreAsync(CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Invocation {Invocations} of DummyDemoDataGenerator happens",
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

public class DummyDemoDataGeneratorSpy
{
    public int InvocationCount;
    public bool ShouldRun = true;
}