using System.Linq;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Features.DataGeneration;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.DataGeneration;

public class TheDataGenerationFeatureWithSimpleInjector : TheDataGenerationFeature
{
    public TheDataGenerationFeatureWithSimpleInjector(ITestOutputHelper output) 
        : base(new SimpleInjectorCompositionRoot(), output)
    {
    }
}

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
    private readonly DummyServicesFeature _dummyServicesFeature = new();

    protected TheDataGenerationFeature(ICompositionRoot compositionRoot, ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(compositionRoot, _exceptionLogger, GetType().Assembly);
    }

    [Fact]
    public async Task HasInjectedDataGenerationContextAndGenerators()
    {
        _sut.EnableFeature(new DataGenerationFeature());
        _sut.EnableFeature(_dummyServicesFeature);

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
        _sut.EnableFeature(_dummyServicesFeature);

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
        _sut.EnableFeature(_dummyServicesFeature);

        await _sut.BootAsync();

        Assert.Equal(1, _dummyServicesFeature.Spies.DummyProductiveDataGeneratorSpy.InvocationCount);
        Assert.Equal(allowDemoDataGeneration ? 1 : 0,
            _dummyServicesFeature.Spies.DummyDemoDataGeneratorSpy.InvocationCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DataGeneratorsDoNotRunWhenShouldRunReturnsFalse(bool allowDemoDataGeneration)
    {
        _dummyServicesFeature.Spies.DummyDemoDataGeneratorSpy.ShouldRun = false;
        _dummyServicesFeature.Spies.DummyProductiveDataGeneratorSpy.ShouldRun = false;

        _sut.EnableFeature(new DataGenerationFeature(allowDemoDataGeneration));
        _sut.EnableFeature(_dummyServicesFeature);
        await _sut.BootAsync();

        Assert.Equal(0, _dummyServicesFeature.Spies.DummyDemoDataGeneratorSpy.InvocationCount);
        Assert.Equal(0, _dummyServicesFeature.Spies.DummyProductiveDataGeneratorSpy.InvocationCount);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sut.Dispose();
        }
    }
}