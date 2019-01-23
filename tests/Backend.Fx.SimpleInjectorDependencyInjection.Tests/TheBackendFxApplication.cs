using System.Linq;
using System.Threading.Tasks;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
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
        public async Task RunsProdAndDemoDataGeneratorsOnEveryBoot()
        {
            TenantId demoTenantId;
            TenantId prodTenantId;
            using (var sut = CreateSystemUnderTest())
            {
                await sut.Boot();

                demoTenantId = sut.DemoTenantId;
                using (var scope = sut.ScopeManager.BeginScope(new SystemIdentity(), sut.DemoTenantId))
                {
                    IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Equal(2, allAggregates.Length);
                    Assert.NotNull(allAggregates.SingleOrDefault(agg => agg.Name == ADemoAggregateGenerator.Name));
                    Assert.NotNull(allAggregates.SingleOrDefault(agg => agg.Name == AProdAggregateGenerator.Name));
                }

                prodTenantId = sut.ProdTenantId;
                using (var scope = sut.ScopeManager.BeginScope(new SystemIdentity(), sut.ProdTenantId))
                {
                    IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Equal(1, allAggregates.Length);
                    Assert.NotNull(allAggregates.SingleOrDefault(agg => agg.Name == AProdAggregateGenerator.Name));
                }
            }

            using (var sut = CreateSystemUnderTest())
            {
                await sut.Boot();

                Assert.Equal(demoTenantId, sut.DemoTenantId);
                using (var scope = sut.ScopeManager.BeginScope(new SystemIdentity(), sut.DemoTenantId))
                {
                    IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Equal(4, allAggregates.Length);
                    Assert.Equal(2, allAggregates.Count(agg => agg.Name == ADemoAggregateGenerator.Name));
                    Assert.Equal(2, allAggregates.Count(agg => agg.Name == AProdAggregateGenerator.Name));
                }

                Assert.Equal(prodTenantId, sut.ProdTenantId);
                using (var scope = sut.ScopeManager.BeginScope(new SystemIdentity(), sut.ProdTenantId))
                {
                    IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Equal(2, allAggregates.Length);
                    Assert.Equal(2, allAggregates.Count(agg => agg.Name == AProdAggregateGenerator.Name));
                }
            }
        }

        private AnApplication CreateSystemUnderTest()
        {
            SimpleInjectorCompositionRoot compositionRoot = new SimpleInjectorCompositionRoot();
            compositionRoot.RegisterModules(
                new EventBusModule(new InMemoryEventBus(compositionRoot, new DebugExceptionLogger()),
                    typeof(AggregateRoot).Assembly),
                new ADomainModule(typeof(AnApplication).Assembly, typeof(AggregateRoot).Assembly),
                _persistenceModule);

            var sut = new AnApplication(compositionRoot, compositionRoot, new InMemoryTenantManager());
            return sut;
        }
    }
}
