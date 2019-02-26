using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain;
using FakeItEasy;
using SimpleInjector;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheTenantManager
    {
        private readonly ITenantManager _sut;
        private readonly ICompositionRoot _compositionRoot;

        private class ADomainModule  : DomainModule 
        {
            public ADomainModule(params Assembly[] domainAssemblies) : base(new DebugExceptionLogger(), domainAssemblies)
            { }

            protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
            {
                base.Register(container, scopedLifestyle);
                container.Register<IClock, FrozenClock>();
                container.RegisterInstance(A.Fake<IEventBus>());
            }
        }

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
            _compositionRoot = compositionRoot;
        }

        [Fact]
        public void RaisesTenantCreatedEvent()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            _sut.TenantCreated += (sender, id) => ev.Set();
            Task.Run(() => _sut.CreateProductionTenant("prod", "unit test created", true, new CultureInfo("de-DE")));
            Assert.True(ev.WaitOne(1000));
        }
    }
}
