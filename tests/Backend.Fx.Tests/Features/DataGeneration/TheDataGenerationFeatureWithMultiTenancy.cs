using System.Linq;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Features.DataGeneration;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Features.MultiTenancy.InProc;
using Backend.Fx.Features.MultiTenancyAdmin;
using Backend.Fx.Features.MultiTenancyAdmin.InMem;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.DataGeneration;

[UsedImplicitly]
public class TheDataGenerationFeatureWithMultiTenancyWithSimpleInjector : TheDataGenerationFeatureWithMultiTenancy
{
    public TheDataGenerationFeatureWithMultiTenancyWithSimpleInjector(ITestOutputHelper output)
        : base(new SimpleInjectorCompositionRoot(), output)
    {
    }
}

[UsedImplicitly]
public class TheDataGenerationFeatureWithMultiTenancyWithMicrosoftDI : TheDataGenerationFeatureWithMultiTenancy
{
    public TheDataGenerationFeatureWithMultiTenancyWithMicrosoftDI(ITestOutputHelper output)
        : base(new MicrosoftCompositionRoot(), output)
    {
    }
}

public abstract class TheDataGenerationFeatureWithMultiTenancy : TestWithLogging
{
    private readonly InMemoryTenantRepository _tenantRepository = new();
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    protected TheDataGenerationFeatureWithMultiTenancy(ICompositionRoot compositionRoot, ITestOutputHelper output) :
        base(output)
    {
        _tenantRepository.SaveTenant(new Tenant(1, "t1", "tenant 1", false));
        _tenantRepository.SaveTenant(new Tenant(2, "t2", "tenant 2", true));

        _sut = new MultiTenancyBackendFxApplication<DummyTenantIdSelector>(
            compositionRoot,
            _exceptionLogger,
            new DirectTenantEnumerator(_tenantRepository),
            new InProcTenantWideMutexManager(),
            GetType().Assembly);
    }

    [Fact]
    public async Task HasInjectedDataGenerationContextAndGenerators()
    {
        _sut.EnableFeature(new DataGenerationFeature());
        var demoSpy = new DummyDemoDataGeneratorSpy();
        var prodSpy = new DummyProductiveDataGeneratorSpy();
        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(demoSpy));
        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(prodSpy));
        await _sut.BootAsync();

        var dataGenerationContext = _sut.CompositionRoot.ServiceProvider.GetRequiredService<IDataGenerationContext>();
        Assert.IsType<ForEachTenantDataGenerationContext>(dataGenerationContext);

        var dataGeneratorTypes = await dataGenerationContext.GetDataGeneratorTypesAsync(_sut.Invoker);
        Assert.Contains(dataGeneratorTypes, dgt => typeof(IProductiveDataGenerator).IsAssignableFrom(dgt));
        Assert.Contains(dataGeneratorTypes, dgt => typeof(IDemoDataGenerator).IsAssignableFrom(dgt));
    }

    [Fact]
    public async Task OnlyInjectsProductiveDataGenerationTypesWhenNoDemoDataGenerationIsAllowed()
    {
        _sut.EnableFeature(new DataGenerationFeature(allowDemoDataGeneration: false));
        var demoSpy = new DummyDemoDataGeneratorSpy();
        var prodSpy = new DummyProductiveDataGeneratorSpy();
        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(demoSpy));
        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(prodSpy));
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
        var demoSpy = new DummyDemoDataGeneratorSpy();
        var prodSpy = new DummyProductiveDataGeneratorSpy();

        demoSpy.ShouldRun = false;
        demoSpy.ShouldRun = true;

        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(demoSpy));
        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(prodSpy));
        await _sut.BootAsync();

        Assert.Equal(2, prodSpy.InvocationCount);
        Assert.Equal(allowDemoDataGeneration ? 1 : 0, demoSpy.InvocationCount);
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