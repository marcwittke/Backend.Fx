using System;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.DomainServices;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.DomainServices;

public class TheDomainServiceFeature : TestWithLogging
{
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    public TheDomainServiceFeature(ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(new MicrosoftCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _sut.EnableFeature(new DomainServicesFeature());
    }

    [Fact]
    public async Task InjectsDomainServices()
    {
        await _sut.BootAsync();
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var domainService = sp.GetRequiredService<IDummyDomainService>();
            Assert.IsType<DummyDomainService>(domainService);
            Assert.Equal(DummyDomainService.Message, domainService.SayHelloToDomain());
            return Task.CompletedTask;
        }, new AnonymousIdentity());
    }

    [Fact]
    public async Task InjectsApplicationServices()
    {
        await _sut.BootAsync();
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var applicationService = sp.GetRequiredService<IDummyApplicationService>();
            Assert.IsType<DummyApplicationService>(applicationService);
            Assert.Equal(DummyApplicationService.Message, applicationService.SayHelloToApplication());
            return Task.CompletedTask;
        }, new AnonymousIdentity());
    }

    [Fact]
    public async Task DoesNotProvideSingletonServices()
    {
        await _sut.BootAsync();
        Assert.Throws<InvalidOperationException>(() =>
            _sut.CompositionRoot.ServiceProvider.GetRequiredService<IDummyDomainService>());
    }
}