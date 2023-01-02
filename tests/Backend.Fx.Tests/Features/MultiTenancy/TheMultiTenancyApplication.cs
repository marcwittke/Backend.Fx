using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Features.MultiTenancy.InProc;
using Backend.Fx.Features.MultiTenancyAdmin;
using Backend.Fx.Features.MultiTenancyAdmin.InMem;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.TestUtil;
using Backend.Fx.Util;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.MultiTenancy;

public class TheMultiTenancyApplication : TestWithLogging
{
    private readonly InMemoryTenantRepository _tenantRepository = new();
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    public TheMultiTenancyApplication(ITestOutputHelper output) : base(output)
    {
        _tenantRepository.SaveTenant(new Tenant(1, "t1", "tenant 1", false));
        _tenantRepository.SaveTenant(new Tenant(2, "t2", "tenant 2", true));

        _sut = new MultiTenancyBackendFxApplication<DummyTenantIdSelector>(new MicrosoftCompositionRoot(),
            _exceptionLogger,
            new DirectTenantEnumerator(_tenantRepository), new InProcTenantWideMutexManager(), GetType().Assembly);
    }

    [Fact]
    public async Task InjectsTheTenantIdIntoTheOperationScope()
    {
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var tenantIdHolder = sp.GetRequiredService<ICurrentTHolder<TenantId>>();
            Assert.Equal(new TenantId(1234), tenantIdHolder.Current);
            return Task.CompletedTask;
        }, new AnonymousIdentity());
    }
    

    [UsedImplicitly]
    private class DummyTenantIdSelector : ICurrentTenantIdSelector
    {
        public TenantId GetCurrentTenantId()
        {
            return new TenantId(1234);
        }
    }
}