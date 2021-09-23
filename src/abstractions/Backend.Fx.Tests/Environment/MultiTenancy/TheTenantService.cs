using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheTenantIdProvider
    {
        private readonly IMessageBus _messageBus = A.Fake<IMessageBus>();
        private readonly ITenantIdProvider _sut;
        private readonly InMemoryTenantRepository _tenantRepository = new();

        public TheTenantIdProvider()
        {
            var tenantService = new TenantService(_messageBus, _tenantRepository);
            _sut = new TenantServiceTenantIdProvider(tenantService);
        }

        [Fact]
        public void GetsProductiveTenantIds()
        {
            Tenant[] tenants = Enumerable.Range(1, 7)
                .Select(i => new Tenant("n" + i, "d" + i, i % 2 == 0))
                .ToArray();

            foreach (var tenant in tenants)
            {
                tenant.State = TenantState.Active;
                _tenantRepository.SaveTenant(tenant);
            }

            TenantId[] tenantIds = tenants.Select(t => new TenantId(t.Id)).ToArray();
            TenantId[] demoTenantIds = tenants.Where(t => t.IsDemoTenant).Select(t => new TenantId(t.Id)).ToArray();
            TenantId[] prodTenantIds = tenants.Where(t => !t.IsDemoTenant).Select(t => new TenantId(t.Id)).ToArray();

            Assert.Equal(tenantIds, _sut.GetActiveTenantIds());
            Assert.Equal(prodTenantIds, _sut.GetActiveProductionTenantIds());
            Assert.Equal(demoTenantIds, _sut.GetActiveDemonstrationTenantIds());
        }
    }


    public class TheTenantService
    {
        private readonly IMessageBus _messageBus = A.Fake<IMessageBus>();

        private readonly ITenantService _sut;
        private readonly InMemoryTenantRepository _tenantRepository = new();

        public TheTenantService()
        {
            _sut = new TenantService(_messageBus, _tenantRepository);
        }

        [Fact]
        public void CannotCreateTenantWithoutName()
        {
            Assert.Throws<ArgumentException>(() => _sut.CreateTenant("", "d", true));
            Assert.Throws<ArgumentException>(() => _sut.CreateTenant(null, "d", true));
        }

        [Fact]
        public void CannotCreateTenantWithSameName()
        {
            _sut.CreateTenant("n", "d", true);
            Assert.Throws<ArgumentException>(() => _sut.CreateTenant("n", "d", true));
            Assert.Throws<ArgumentException>(() => _sut.CreateTenant("n", "d", false));
            Assert.Throws<ArgumentException>(() => _sut.CreateTenant("N", "d", true));
        }

        [Fact]
        public void RaisesTenantActivatedEvent()
        {
            var ev = new ManualResetEvent(false);
            A.CallTo(() => _messageBus.Publish(A<TenantActivated>._)).Invokes(() => ev.Set());
            Task.Run(() => _sut.CreateTenant("prod", "unit test created", false));
            Assert.True(ev.WaitOne(Debugger.IsAttached ? int.MaxValue : 10000));
        }
    }
}
