using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Exceptions;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.MultiTenancy
{
    public class TheTenantService
    {
        private readonly ITenantService _sut;
        private readonly InMemoryTenantRepository _tenantRepository = new InMemoryTenantRepository();
        private readonly IMessageBus _messageBus = A.Fake<IMessageBus>();
        
        public TheTenantService()
        {
            _sut = new TenantService(_messageBus, _tenantRepository);
        }

        [Fact]
        public void CanGetTenants()
        {
            _sut.CreateTenant("t1", "d1", false);
            var inactive = _sut.CreateTenant("t2", "d2", false);
            _sut.CreateTenant("t3", "d3", false);
            _sut.CreateTenant("t4", "d4", true);
            
            _sut.DeactivateTenant(inactive);
            
            
            var tenants = _sut.GetTenants();
            Assert.Equal(4, tenants.Length);
            Assert.Contains(tenants, t => t.Name == "t1");
            Assert.Contains(tenants, t => t.Name == "t2");
            Assert.Contains(tenants, t => t.Name == "t3");
            Assert.Contains(tenants, t => t.Name == "t4");
            
            tenants = _sut.GetActiveTenants();
            Assert.Equal(3, tenants.Length);
            Assert.Contains(tenants, t => t.Name == "t1");
            Assert.Contains(tenants, t => t.Name == "t3");
            Assert.Contains(tenants, t => t.Name == "t4");
            
            tenants = _sut.GetActiveProductionTenants();
            Assert.Equal(2, tenants.Length);
            Assert.Contains(tenants, t => t.Name == "t1");
            Assert.Contains(tenants, t => t.Name == "t3");
            
            tenants = _sut.GetActiveDemonstrationTenants();
            Assert.Single(tenants);
            Assert.Contains(tenants, t => t.Name == "t4");
        }
        
        [Fact]
        public void CanGetTenant()
        {
            var tenantId = _sut.CreateTenant("t1", "d1", false);
            var tenant = _sut.GetTenant(tenantId);
            Assert.Equal(tenantId.Value, tenant.Id);
            
            _sut.DeactivateTenant(tenantId);
            tenant = _sut.GetTenant(tenantId);
            Assert.Equal(tenantId.Value, tenant.Id);
        }
        
        [Fact]
        public void CanDeactivateAndActivate()
        {
            var tenantId = _sut.CreateTenant("t1", "d1", false);
            Fake.ClearRecordedCalls(_messageBus);
            
            _sut.DeactivateTenant(tenantId);
            A.CallTo(() => _messageBus.Publish(A<TenantDeactivated>._)).MustHaveHappenedOnceExactly();
            Assert.Equal(TenantState.Inactive, _tenantRepository.GetTenant(tenantId).State);
            
            _sut.ActivateTenant(tenantId);
            A.CallTo(() => _messageBus.Publish(A<TenantActivated>._)).MustHaveHappenedOnceExactly();
            Assert.Equal(TenantState.Active, _tenantRepository.GetTenant(tenantId).State);
        }

        [Fact]
        public void CannotDeleteActiveTenant()
        {
            var tenantId = _sut.CreateTenant("t1", "d1", false);
            Assert.Throws<UnprocessableException>(() => _sut.DeleteTenant(tenantId));
        }
        
        [Fact]
        public void CanDelete()
        {
            var tenantId = _sut.CreateTenant("t1", "d1", false);
            
            _sut.DeactivateTenant(tenantId);
            _sut.DeleteTenant(tenantId);
            
            Assert.Empty(_sut.GetTenants());
        }
    }
}