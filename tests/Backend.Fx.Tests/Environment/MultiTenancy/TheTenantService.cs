using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheTenantService
    {
        private readonly ITenantService _sut;
        private readonly IMessageBus _messageBus = A.Fake<IMessageBus>();
        private readonly InMemoryTenantRepository _tenantRepository = new InMemoryTenantRepository();

        public TheTenantService()
        {
            _sut = new TenantService(_messageBus, _tenantRepository);
        }

        [Fact]
        public void RaisesTenantCreatedEvent()
        {
            var ev = new ManualResetEvent(false);
            A.CallTo(() => _messageBus.Publish(A<TenantCreated>._)).Invokes(() => ev.Set());
            Task.Run(() => _sut.CreateTenant(new TenantCreationParameters { Name="prod",Description= "unit test created",DefaultCultureName = "de-DE", IsDemonstrationTenant = false}));
            Assert.True(ev.WaitOne(Debugger.IsAttached ? int.MaxValue : 10000));
        }

        [Fact]
        public void RaisesTenantActivatedEvent()
        {
            FakeAutoActivation();
            var ev = new ManualResetEvent(false);
            A.CallTo(() => _messageBus.Publish(A<TenantActivated>._)).Invokes(() => ev.Set());
            Task.Run(() => _sut.CreateTenant(new TenantCreationParameters { Name="prod",Description= "unit test created",DefaultCultureName = "de-DE", IsDemonstrationTenant = false}));
            Assert.True(ev.WaitOne(Debugger.IsAttached ? int.MaxValue : 10000));
        }

        [Fact]
        public void CannotCreateTenantWithSameName()
        {
            _sut.CreateTenant(new TenantCreationParameters("n", "d", true, "de-DE"));
            Assert.Throws<ArgumentException>(()=>_sut.CreateTenant(new TenantCreationParameters("n", "d", true, "de-DE")));
            Assert.Throws<ArgumentException>(()=>_sut.CreateTenant(new TenantCreationParameters("n", "d", false, "de-DE")));
            Assert.Throws<ArgumentException>(()=>_sut.CreateTenant(new TenantCreationParameters("N", "d", true, "de-DE")));
        }

        [Fact]
        public void CannotCreateTenantWithoutName()
        {
            Assert.Throws<ArgumentException>(()=>_sut.CreateTenant(new TenantCreationParameters("", "d", true, "de-DE")));
            Assert.Throws<ArgumentException>(()=>_sut.CreateTenant(new TenantCreationParameters(null, "d", true, "de-DE")));
        }
        
        [Fact]
        public void GetsProductiveTenantIds()
        {
            var tenants = Enumerable.Range(1, 7)
                      .Select(i => new Tenant("n" + i, "d" + i, i % 2 == 0, "de-DE"))
                      .ToArray();
            
            foreach (Tenant tenant in tenants)
            {
                tenant.State = TenantState.Active;
                _tenantRepository.SaveTenant(tenant);
            }
        
            var tenantIds = tenants.Select(t => new TenantId(t.Id)).ToArray();
            var demoTenantIds = tenants.Where(t => t.IsDemoTenant).Select(t => new TenantId(t.Id)).ToArray();
            var prodTenantIds = tenants.Where(t => !t.IsDemoTenant).Select(t => new TenantId(t.Id)).ToArray();
            
            Assert.Equal(tenantIds, _sut.GetActiveTenantIds());
            Assert.Equal(prodTenantIds, _sut.GetActiveProductionTenantIds());
            Assert.Equal(demoTenantIds, _sut.GetActiveDemonstrationTenantIds());
        }

        private void FakeAutoActivation()
        {
            A.CallTo(() => _messageBus.Publish(A<IIntegrationEvent>._)).Invokes((IIntegrationEvent iev) =>
            {
                _sut.ActivateTenant(new TenantId(iev.TenantId));
            });
        }
    }
}
