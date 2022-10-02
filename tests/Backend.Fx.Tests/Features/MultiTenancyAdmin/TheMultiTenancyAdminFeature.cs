using System.Threading.Tasks;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Features.MultiTenancyAdmin;
using Backend.Fx.Features.MultiTenancyAdmin.InMem;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.TestUtil;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.MultiTenancyAdmin;

public class TheMultiTenancyAdminFeature : TestWithLogging
{
    private readonly InMemoryTenantRepository _tenantRepository = new();
    private readonly IBackendFxApplication _sut;
    
    public TheMultiTenancyAdminFeature(ITestOutputHelper output) : base(output)
    {
        _sut = new MultiTenancyBackendFxApplication<DummyTenantIdSelector>(
            new MicrosoftCompositionRoot(),
            new DirectTenantEnumerator(_tenantRepository));
    }

    [Fact]
    public async Task DoesNotCreateTenantsOnBoot()
    {
        _sut.EnableFeature(new MultiTenancyAdminFeature(_tenantRepository, ensureDemoTenantOnBoot: false));
        await _sut.BootAsync();

        var tenants = _tenantRepository.GetTenants();
        Assert.Empty(tenants);
    }
    
    [Fact]
    public async Task CreatesDemoTenantOnBootWhenRequested()
    {
        _sut.EnableFeature(new MultiTenancyAdminFeature(_tenantRepository, ensureDemoTenantOnBoot: true));
        await _sut.BootAsync();

        var tenants = _tenantRepository.GetTenants();
        Assert.Single(tenants);
        Assert.True(tenants[0].IsDemoTenant);
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