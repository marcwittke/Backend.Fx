using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Features.DataGeneration;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.DataGeneration;

[UsedImplicitly]
[Collection("Data Generation Collection")]
public class TheDataGenerationFeatureWithSimpleInjector : TheDataGenerationFeature
{
    public TheDataGenerationFeatureWithSimpleInjector(ITestOutputHelper output)
        : base(new SimpleInjectorCompositionRoot(), output)
    {
    }
}

[UsedImplicitly]
[Collection("Data Generation Collection")]
public class TheDataGenerationFeatureWithMicrosoftDI : TheDataGenerationFeature
{
    public TheDataGenerationFeatureWithMicrosoftDI(ITestOutputHelper output)
        : base(new MicrosoftCompositionRoot(), output)
    {
    }
}

public abstract class TheDataGenerationFeature : TestWithLogging
{
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    protected TheDataGenerationFeature(ICompositionRoot compositionRoot, ITestOutputHelper output) :
        base(output)
    {
        Fake.ClearRecordedCalls(DataGenerationSpiesFixture.SomeDemoDataGenerator.Spy);
        Fake.ClearRecordedCalls(DataGenerationSpiesFixture.SomeProductiveDataGenerator.Spy);
        
        _sut = new BackendFxApplication(compositionRoot, _exceptionLogger, GetType().Assembly);
    }

    [Fact]
    public async Task HasInjectedDataGenerationContextAndGenerators()
    {
        _sut.EnableFeature(new DataGenerationFeature());
        await _sut.BootAsync();

        var dataGenerationContext = _sut.CompositionRoot.ServiceProvider.GetRequiredService<IDataGenerationContext>();
        Assert.IsType<DataGenerationContext>(dataGenerationContext);

        var dataGeneratorTypes = await dataGenerationContext.GetDataGeneratorTypesAsync(_sut.Invoker);
        Assert.Contains(dataGeneratorTypes, dgt => typeof(IProductiveDataGenerator).IsAssignableFrom(dgt));
        Assert.Contains(dataGeneratorTypes, dgt => typeof(IDemoDataGenerator).IsAssignableFrom(dgt));
    }

    [Fact]
    public async Task OnlyInjectsProductiveDataGenerationTypesWhenNoDemoDataGenerationIsAllowed()
    {
        _sut.EnableFeature(new DataGenerationFeature(allowDemoDataGeneration: false));
        await _sut.BootAsync();
        var dataGenerationContext = _sut.CompositionRoot.ServiceProvider.GetRequiredService<IDataGenerationContext>();
        var dataGeneratorTypes = await dataGenerationContext.GetDataGeneratorTypesAsync(_sut.Invoker);
        Assert.True(dataGeneratorTypes.All(dgt => typeof(IProductiveDataGenerator).IsAssignableFrom(dgt)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GeneratesDataOnBoot(bool allowDemoDataGeneration)
    {
        _sut.EnableFeature(new DataGenerationFeature(allowDemoDataGeneration));
        await _sut.BootAsync();
        
        A.CallTo(() => DataGenerationSpiesFixture.SomeProductiveDataGenerator.Spy.GenerateAsync(A<CancellationToken>._)).MustHaveHappened(1, Times.Exactly);
        
        var expectedDemoGeneratorCalls = (allowDemoDataGeneration ? 1 : 0);
        A.CallTo(() => DataGenerationSpiesFixture.SomeDemoDataGenerator.Spy.GenerateAsync(A<CancellationToken>._)).MustHaveHappened(expectedDemoGeneratorCalls, Times.Exactly);
    }


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sut.Dispose();
        }
    }

    [UsedImplicitly]
    private class DummyTenantIdSelector : ICurrentTenantIdSelector
    {
        public TenantId GetCurrentTenantId()
        {
            return new TenantId(1000);
        }
    }
}

