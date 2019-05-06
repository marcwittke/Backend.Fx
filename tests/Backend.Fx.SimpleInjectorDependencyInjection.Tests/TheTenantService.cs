using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheTenantService
    {
        private readonly ITenantService _sut;
        private readonly IEventBus _eventBus = A.Fake<IEventBus>();
        
        public TheTenantService()
        {
            A.CallTo(() => _eventBus.Publish(A<IIntegrationEvent>._)).Invokes((IIntegrationEvent iev) =>
            {
                _sut.ActivateTenant(new TenantId(iev.TenantId));
            });

            var compositionRoot = new SimpleInjectorCompositionRoot();
            var domainAssembly = typeof(AnAggregate).GetTypeInfo().Assembly;
            var backendfxAssembly = typeof(Entity).GetTypeInfo().Assembly;
            compositionRoot.RegisterModules(
                new InfrastructureModule(new DebugExceptionLogger(), _eventBus),
                new ADomainModule(domainAssembly, backendfxAssembly),
                new APersistenceModule(domainAssembly));

            compositionRoot.Verify();

            _sut = new TenantService(_eventBus, new InMemoryTenantRepository());
        }

        [Fact]
        public void RaisesTenantCreatedEvent()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            A.CallTo(() => _eventBus.Publish(A<TenantCreated>._)).Invokes(() => ev.Set());
            Task.Run(() => _sut.CreateProductionTenant("prod", "unit test created", "de-DE"));
            Assert.True(ev.WaitOne(Debugger.IsAttached ? int.MaxValue : 10000));
        }

        [Fact]
        public void RaisesTenantActivatedEvent()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            A.CallTo(() => _eventBus.Publish(A<TenantActivated>._)).Invokes(() => ev.Set());

            Task.Run(() => _sut.CreateProductionTenant("prod", "unit test created", "de-DE"));
            Assert.True(ev.WaitOne(Debugger.IsAttached ? int.MaxValue : 10000));
        }
    }
}
