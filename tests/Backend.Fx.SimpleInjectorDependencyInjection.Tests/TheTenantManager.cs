using System.Globalization;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl;
using Backend.Fx.Testing.InMemoryPersistence;
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
            public ADomainModule(params Assembly[] domainAssemblies) : base(domainAssemblies)
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

            _sut = new InMemoryTenantManager(new TenantInitializer(compositionRoot));
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

        [Fact]
        public void RunsProductiveDataGeneratorsOnTenantInitialization()
        {
            TenantId tenantId = _sut.CreateProductionTenant("prod", "unit test created", true, new CultureInfo("en-US"));
            _sut.EnsureTenantIsInitialized(tenantId);

            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), tenantId))
            {
                IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                AnAggregate[] allAggregates = repository.GetAll();
                Assert.Single(allAggregates);
                Assert.Equal("Productive record", allAggregates[0].Name);
            }
        }

        [Fact]
        public void RunsProductiveAndDemonstrationDataGeneratorsOnDemoTenantInitialization()
        {
            TenantId tenantId = _sut.CreateDemonstrationTenant("demo", "unit test created", true, new CultureInfo("en-US"));
            _sut.EnsureTenantIsInitialized(tenantId);

            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), tenantId))
            {
                IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                AnAggregate[] allAggregates = repository.GetAll();
                Assert.Equal(2, allAggregates.Length);
                Assert.Equal("Productive record", allAggregates[0].Name);
                Assert.Equal("Demo record", allAggregates[1].Name);
            }
        }
    }
}
