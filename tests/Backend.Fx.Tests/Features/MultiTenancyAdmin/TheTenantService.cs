using Backend.Fx.Exceptions;
using Backend.Fx.Features.MultiTenancyAdmin;
using Backend.Fx.Features.MultiTenancyAdmin.InMem;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.MultiTenancyAdmin;

public class TheTenantService : TestWithLogging
{
    public TheTenantService(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CreatesTenantsActive()
    {
        var repo = new InMemoryTenantRepository();
        var sut = new TenantService(repo);

        Tenant? tenant = sut.CreateTenant("T1", "Tenant 1", false);
        Assert.True(tenant.IsActive);
    }
    
    [Fact]
    public void ProvidesTenants()
    {
        var repo = new InMemoryTenantRepository();
        repo.SaveTenant(new Tenant(1, "t1", "tenant 1", false));
        repo.SaveTenant(new Tenant(2, "t2", "tenant 2", true));
        
        var sut = new TenantService(repo);
        
        var tenant = sut.GetTenant(1);
        Assert.Equal(1, tenant.Id);

        Assert.Throws<NotFoundException<Tenant>>(()=>sut.GetTenant(99));
        
        var tenants = sut.GetActiveTenants();
        Assert.Equal(2, tenants.Length);
        
        tenants = sut.GetActiveDemonstrationTenants();
        Assert.Single(tenants);
        Assert.Equal(2, tenants[0].Id);
        
        tenants = sut.GetActiveProductionTenants();
        Assert.Single(tenants);
        Assert.Equal(1, tenants[0].Id);

        sut.DeactivateTenant(1);
        
        tenants = sut.GetActiveTenants();
        Assert.Single(tenants);
        Assert.Equal(2, tenants[0].Id);
        
        sut.DeactivateTenant(2);
        
        tenants = sut.GetActiveTenants();
        Assert.Empty(tenants);
        
        tenants = sut.GetActiveDemonstrationTenants();
        Assert.Empty(tenants);
        
        tenants = sut.GetActiveProductionTenants();
        Assert.Empty(tenants);
        
        
    }
    
    [Fact]
    public void CannotDeleteActiveTenant()
    {
        var repo = new InMemoryTenantRepository();
        var sut = new TenantService(repo);

        Tenant? tenant = sut.CreateTenant("T1", "Tenant 1", false);
        Assert.True(tenant.IsActive);

        Assert.Throws<UnprocessableException>(() => sut.DeleteTenant(tenant.Id));
    }
    
    [Fact]
    public void CanDeleteInactiveTenant()
    {
        var repo = new InMemoryTenantRepository();
        var sut = new TenantService(repo);

        Tenant? tenant = sut.CreateTenant("T1", "Tenant 1", false);
        Assert.True(tenant.IsActive);
        sut.DeactivateTenant(tenant.Id);
        sut.DeleteTenant(tenant.Id);
        Assert.Empty(repo.GetTenants());
    }
    
    [Fact]
    public void CanDeactivateAndActivateTenants()
    {
        var repo = new InMemoryTenantRepository();
        var sut = new TenantService(repo);

        Tenant? tenant = sut.CreateTenant("T1", "Tenant 1", false);
        
        sut.DeactivateTenant(tenant.Id);
        Assert.False(tenant.IsActive);
        sut.ActivateTenant(tenant.Id);
        Assert.True(tenant.IsActive);
    }
}