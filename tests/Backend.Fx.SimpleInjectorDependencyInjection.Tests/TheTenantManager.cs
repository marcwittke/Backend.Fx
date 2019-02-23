using System.Globalization;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
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
        private readonly IScopeManager _scopeManager;

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
            _scopeManager = compositionRoot;
        }

        [Fact]
        public void RunsNoDataGeneratorsOnTenantCreation()
        {
            TenantId tenantId = _sut.CreateProductionTenant("prod", "unit test created", true, new CultureInfo("de-DE"));

            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), tenantId))
            {
                IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                AnAggregate[] allAggregates = repository.GetAll();
                Assert.Empty(allAggregates);
            }
        }
    }
}
