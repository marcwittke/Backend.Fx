namespace Backend.Fx.Bootstrapping.Tests
{
    using System.Globalization;
    using System.Reflection;
    using BuildingBlocks;
    using DummyImpl;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Modules;
    using Patterns.DependencyInjection;
    using SimpleInjector;
    using Testing.InMemoryPersistence;
    using Xunit;

    public class TheTenantManager
    {
        private readonly ITenantManager sut;
        private readonly IScopeManager scopeManager;

        private class AnApplicationModule  : ApplicationModule 
        {
            public AnApplicationModule(params Assembly[] domainAssemblies) : base(domainAssemblies)
            { }

            protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
            {
                base.Register(container, scopedLifestyle);
                container.Register<IClock, FrozenClock>();
            }
        }

        public TheTenantManager()
        {
            var compositionRoot = new SimpleInjectorCompositionRoot();
            var domainAssembly = typeof(AnAggregate).GetTypeInfo().Assembly;
            compositionRoot.RegisterModules(
                new AnApplicationModule(domainAssembly),
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
                Assert.Equal(0, allAggregates.Length);
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
                Assert.Equal(1, allAggregates.Length);
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



    //[Fact]
    //    public void HandlesIntegrationEvents()
    //    {
    //        bool wasHandled = false;
    //        sut.Boot();
    //        TenantId tenantId = sut.TenantManager.CreateDemonstrationTenant("for integration event test", "", true);

    //        sut.SubscribeToIntegrationEvent<TheIntegrationEvent>(evt =>
    //        {
    //            Assert.Equal(tenantId.Value, evt.TenantId);
    //            Assert.Equal(42, evt.Whatever);
    //            wasHandled = true;
    //        });

    //        sut.GetInstance<IEventAggregator>().PublishIntegrationEvent(new TheIntegrationEvent(tenantId.Value, 42)).Wait();
    //        Assert.True(wasHandled);
    //    }
    //}
}
