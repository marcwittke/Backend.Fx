using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.Jobs;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.Jobs;

public class TheJobsFeature : TestWithLogging
{
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly DummyServicesFeature _dummyServicesFeature = new ();

    public TheJobsFeature(ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(new MicrosoftCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _sut.EnableFeature(_dummyServicesFeature);
    }

    [Fact]
    public async Task HasInjectedJobExecutor()
    {
        _sut.EnableFeature(new JobsFeature());
        await _sut.BootAsync();
        var jobExecutor = _sut.CompositionRoot.ServiceProvider.GetRequiredService<IJobExecutor>();
        Assert.IsType<JobExecutor>(jobExecutor);
    }
    
    [Fact]
    public async Task CanExecuteJob()
    {
        _sut.EnableFeature(new JobsFeature());
        await _sut.BootAsync();
        await _sut.ExecuteJob<DummyJob>();
        A.CallTo(() => _dummyServicesFeature.Spies.DummyJobSpy.RunAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}