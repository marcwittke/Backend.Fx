using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.Jobs;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Features.MultiTenancy.InProc;
using Backend.Fx.Features.MultiTenancyAdmin;
using Backend.Fx.Features.MultiTenancyAdmin.InMem;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.Jobs;

public class TheJobsFeatureWithMultiTenancy : TestWithLogging
{
    private readonly InMemoryTenantRepository _tenantRepository = new();
    
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly DummyServicesFeature _dummyServicesFeature = new ();

    public TheJobsFeatureWithMultiTenancy(ITestOutputHelper output) : base(output)
    {
        _tenantRepository.SaveTenant(new Tenant(1, "t1", "tenant 1", false));
        _tenantRepository.SaveTenant(new Tenant(2, "t2", "tenant 2", false));
        
        _sut = new MultiTenancyBackendFxApplication<DummyTenantIdSelector>(new MicrosoftCompositionRoot(),
            _exceptionLogger,
            new DirectTenantEnumerator(_tenantRepository), new InProcTenantWideMutexManager(), GetType().Assembly);
        
        _sut.EnableFeature(_dummyServicesFeature);
    }

    [Fact]
    public async Task HasInjectedJobExecutor()
    {
        _sut.EnableFeature(new JobsFeature());
        await _sut.BootAsync();
        var jobExecutor = _sut.CompositionRoot.ServiceProvider.GetRequiredService<IJobExecutor>();
        Assert.IsType<ForEachTenantJobExecutor>(jobExecutor);
    }
    
    [Fact]
    public async Task CanExecuteJobForEachTenant()
    {
        _sut.EnableFeature(new JobsFeature());
        await _sut.BootAsync();
        await _sut.ExecuteJob<DummyJob>();
        A.CallTo(() => _dummyServicesFeature.Spies.DummyJobSpy.RunAsync(A<CancellationToken>._)).MustHaveHappened(2, Times.Exactly);
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