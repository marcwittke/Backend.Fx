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
    private readonly ILogger _logger = Log.Create<DummyProductiveDataGenerator>();
    
    public override int Priority => 1;
    
    protected override Task GenerateCoreAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override void Initialize()
    { }

    protected override bool ShouldRun() => true;
}


public class DummyProductiveDataGeneratorSpy
{
    public int InvocationCount;
    public bool ShouldRun = true;
}