using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheTenantManager
    {
        private readonly ITenantManager _sut;
        
        public TheTenantManager()
        {
            var compositionRoot = new SimpleInjectorCompositionRoot();
            var domainAssembly = typeof(AnAggregate).GetTypeInfo().Assembly;
            var backendfxAssembly = typeof(Entity).GetTypeInfo().Assembly;
            compositionRoot.RegisterModules(
                new ADomainModule(domainAssembly, backendfxAssembly),
                new APersistenceModule(domainAssembly));

            compositionRoot.Verify();

            _sut = new InMemoryTenantManager();
        }

        [Fact]
        public void RaisesTenantCreatedEvent()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            _sut.TenantCreated += (sender, id) => ev.Set();
            Task.Run(() => _sut.CreateProductionTenant("prod", "unit test created", true, new CultureInfo("de-DE")));
            Assert.True(ev.WaitOne(Debugger.IsAttached ? int.MaxValue : 10000));
        }

        [Fact]
        public void RaisesTenantActivatedEvent()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            _sut.TenantCreated += (sender, id) =>
            {
                var tenant = _sut.GetTenant(id);
                _sut.ActivateTenant(tenant);
            };

            _sut.TenantActivated += (sender, id) => ev.Set();
            Task.Run(() => _sut.CreateProductionTenant("prod", "unit test created", true, new CultureInfo("de-DE")));
            Assert.True(ev.WaitOne(Debugger.IsAttached ? int.MaxValue : 10000));
        }
    }
}
