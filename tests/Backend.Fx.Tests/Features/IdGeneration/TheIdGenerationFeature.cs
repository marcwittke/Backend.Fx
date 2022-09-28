using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.IdGeneration;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.IdGeneration;

public class TheIdGenerationFeature : TestWithLogging
{
    private readonly DummyServicesFeature _dummyServicesFeature = new();
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    public TheIdGenerationFeature(ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _sut.EnableFeature(_dummyServicesFeature);
        _sut.EnableFeature(new IdGenerationFeature(_dummyServicesFeature.Spies.EntityIdGenerator));
    }

    [Fact]
    public async Task InjectsTheSingletonEntityIdGenerator()
    {
        await _sut.BootAsync();
        var entityIdGenerator = _sut.CompositionRoot.ServiceProvider.GetRequiredService<IEntityIdGenerator>();
        Assert.StrictEqual(_dummyServicesFeature.Spies.EntityIdGenerator, entityIdGenerator);

        await _sut.Invoker.InvokeAsync(sp =>
        {
            entityIdGenerator = sp.GetRequiredService<IEntityIdGenerator>();
            Assert.StrictEqual(_dummyServicesFeature.Spies.EntityIdGenerator, entityIdGenerator);
            return Task.CompletedTask;
        }, new AnonymousIdentity());
    }
}