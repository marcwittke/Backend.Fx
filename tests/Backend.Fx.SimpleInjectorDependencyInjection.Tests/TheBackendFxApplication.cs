using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheBackendFxApplication
    {
        private readonly APersistenceModule _persistenceModule;

        public TheBackendFxApplication()
        {
            _persistenceModule = new APersistenceModule(typeof(AnApplication).Assembly, typeof(InMemoryStore<>).Assembly);
        }

        [Fact]
        public async Task RunsProdDataGeneratorsOnEveryBoot()
        {
            using (var sut = CreateSystemUnderTest())
            {
                await sut.Boot();

                var tenantHelper = new TenantHelper();
                tenantHelper.EnsureProdTenant(sut);
                
                using (sut.BeginScope(new SystemIdentity(), tenantHelper.ProdTenantId))
                {
                    IRepository<AnAggregate> repository = sut.CompositionRoot.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Equal(1, allAggregates.Length);
                    Assert.Equal(1, allAggregates.Count(agg => agg.Name == AProdAggregateGenerator.Name));
                }
            }

            using (var sut = CreateSystemUnderTest())
            {
                await sut.Boot();

                var tenantHelper = new TenantHelper();
                tenantHelper.EnsureProdTenant(sut);

                using (sut.BeginScope(new SystemIdentity(), tenantHelper.ProdTenantId))
                {
                    IRepository<AnAggregate> repository = sut.CompositionRoot.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Equal(2, allAggregates.Length);
                    Assert.Equal(2, allAggregates.Count(agg => agg.Name == AProdAggregateGenerator.Name));
                }
            }
        }

        [Fact]
        public async Task RunsDemoDataGeneratorsOnEveryBoot()
        {
            using (var sut = CreateSystemUnderTest())
            {
                await sut.Boot();

                var tenantHelper = new TenantHelper();
                tenantHelper.EnsureDemoTenant(sut);

                using (sut.BeginScope(new SystemIdentity(), tenantHelper.DemoTenantId))
                {
                    IRepository<AnAggregate> repository = sut.CompositionRoot.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Equal(2, allAggregates.Length);
                    Assert.NotNull(allAggregates.SingleOrDefault(agg => agg.Name == ADemoAggregateGenerator.Name));
                    Assert.NotNull(allAggregates.SingleOrDefault(agg => agg.Name == AProdAggregateGenerator.Name));
                }
            }

            using (var sut = CreateSystemUnderTest())
            {
                await sut.Boot();

                var tenantHelper = new TenantHelper();
                tenantHelper.EnsureDemoTenant(sut);

                using (sut.BeginScope(new SystemIdentity(), tenantHelper.DemoTenantId))
                {
                    IRepository<AnAggregate> repository = sut.CompositionRoot.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Equal(4, allAggregates.Length);
                    Assert.Equal(2, allAggregates.Count(agg => agg.Name == ADemoAggregateGenerator.Name));
                    Assert.Equal(2, allAggregates.Count(agg => agg.Name == AProdAggregateGenerator.Name));
                }
            }
        }

        [Fact]
        public async Task MaintainsTenantIdWhenBeginningScopes()
        {
            using (var sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                Enumerable.Range(1, 100).AsParallel().ForAll(i =>
                {
                    using (sut.BeginScope(new SystemIdentity(), new TenantId(i)))
                    {
                        var insideScopeTenantId = sut.CompositionRoot.GetInstance<ICurrentTHolder<TenantId>>().Current;
                        Assert.True(insideScopeTenantId.HasValue);
                        Assert.Equal(i, insideScopeTenantId.Value);
                    }
                });
            }
        }

        [Fact]
        public async Task MaintainsIdentityWhenBeginningScopes()
        {
            using (var sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                Enumerable.Range(1, 100).AsParallel().ForAll(i =>
                {
                    using (sut.BeginScope(new GenericIdentity(i.ToString()), new TenantId(100)))
                    {
                        var insideScopeIdentity = sut.CompositionRoot.GetInstance<ICurrentTHolder<IIdentity>>().Current;
                        Assert.Equal(i.ToString(), insideScopeIdentity.Name);
                    }
                });
            }
        }

        private AnApplication CreateSystemUnderTest()
        {
            SimpleInjectorCompositionRoot compositionRoot = new SimpleInjectorCompositionRoot();
            compositionRoot.RegisterModules(
                new ADomainModule(typeof(AnApplication).Assembly, typeof(AggregateRoot).Assembly),
                _persistenceModule);

            var sut = new AnApplication(compositionRoot);
            return sut;
        }

        private class TenantHelper
        {
            public void EnsureProdTenant(IBackendFxApplication application)
            {
                
                var tenants = application.TenantManager.GetTenants();
                var prodTenantId = tenants.SingleOrDefault(t => t.Name == "prod")?.Id;
                if (prodTenantId == null)
                {
                    ManualResetEventSlim prodTenantActivated = new ManualResetEventSlim(false);
                    application.TenantManager.TenantActivated += (_, tenantId) => { prodTenantActivated.Set(); };
                    ProdTenantId = application.TenantManager.CreateProductionTenant("prod", "unit test created", true, new CultureInfo("en-US"));
                    Assert.True(prodTenantActivated.Wait(int.MaxValue));
                }
                else
                {
                    ProdTenantId = new TenantId(prodTenantId.Value);
                }
            }

            public void EnsureDemoTenant(IBackendFxApplication application)
            {
                var tenants = application.TenantManager.GetTenants();
                var demoTenantId = tenants.SingleOrDefault(t => t.Name == "demo")?.Id;
                if (demoTenantId == null)
                {
                    ManualResetEventSlim demoTenantActivated = new ManualResetEventSlim(false);
                    application.TenantManager.TenantActivated += (_, tenantId) => { demoTenantActivated.Set(); };
                    DemoTenantId = application.TenantManager.CreateDemonstrationTenant("demo", "unit test created",
                        false, new CultureInfo("en-US"));
                    Assert.True(demoTenantActivated.Wait(int.MaxValue));
                }
                else
                {
                    DemoTenantId = new TenantId(demoTenantId.Value);
                }
            }

            public TenantId ProdTenantId { get; private set; }

            public TenantId DemoTenantId { get; private set; }
        }
    }
}
