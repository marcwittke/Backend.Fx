namespace Backend.Fx.Bootstrapping.Tests
{
    using System.Globalization;
    using System.Reflection;
    using BuildingBlocks;
    using DummyImpl;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using FakeItEasy;
    using Modules;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation.Integration;
    using SimpleInjector;
    using Testing.InMemoryPersistence;
    using Xunit;

    public class TheTenantManager
    {
        private readonly ITenantManager sut;
        private readonly IScopeManager scopeManager;

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
                new InMemoryIdGeneratorsModule(),
                new InMemoryPersistenceModule(domainAssembly));

            compositionRoot.Verify();

            sut = new InMemoryTenantManager(new TenantInitializer(compositionRoot));
            scopeManager = compositionRoot;
        }

        [Fact]
        public void RunsNoDataGeneratorsOnTenantCreation()
        {
            TenantId tenantId = sut.CreateProductionTenant("prod", "unit test created", true, new CultureInfo("de-DE"));

            using (var scope = scopeManager.BeginScope(new SystemIdentity(), tenantId))
            {
                IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                AnAggregate[] allAggregates = repository.GetAll();
                Assert.Empty(allAggregates);
            }
        }

        [Fact]
        public void RunsProductiveDataGeneratorsOnTenantInitialization()
        {
            TenantId tenantId = sut.CreateProductionTenant("prod", "unit test created", true, new CultureInfo("en-US"));
            sut.EnsureTenantIsInitialized(tenantId);

            using (var scope = scopeManager.BeginScope(new SystemIdentity(), tenantId))
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
            TenantId tenantId = sut.CreateDemonstrationTenant("demo", "unit test created", true, new CultureInfo("en-US"));
            sut.EnsureTenantIsInitialized(tenantId);

            using (var scope = scopeManager.BeginScope(new SystemIdentity(), tenantId))
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
