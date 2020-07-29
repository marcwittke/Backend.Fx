using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
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
        public async Task RunsProdDataGeneratorsOnEveryBoot()
        {
            using (AnApplication sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                sut.EnsureProdTenant();

                using (sut.BeginScope(new SystemIdentity(), sut.ProdTenantId))
                {
                    IRepository<AnAggregate> repository = sut.CompositionRoot.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Single(allAggregates);
                    Assert.Equal(1, allAggregates.Count(agg => agg.Name == AProdAggregateGenerator.Name));
                }
            }

            using (AnApplication sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                sut.EnsureProdTenant();

                using (sut.BeginScope(new SystemIdentity(), sut.ProdTenantId))
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
            using (AnApplication sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                sut.EnsureDemoTenant();
                using (sut.BeginScope(new SystemIdentity(), sut.DemoTenantId))
                {
                    IRepository<AnAggregate> repository = sut.CompositionRoot.GetInstance<IRepository<AnAggregate>>();
                    AnAggregate[] allAggregates = repository.GetAll();
                    Assert.Equal(2, allAggregates.Length);
                    Assert.NotNull(allAggregates.SingleOrDefault(agg => agg.Name == ADemoAggregateGenerator.Name));
                    Assert.NotNull(allAggregates.SingleOrDefault(agg => agg.Name == AProdAggregateGenerator.Name));
                }
            }

            using (AnApplication sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                sut.EnsureDemoTenant();

                using (sut.BeginScope(new SystemIdentity(), sut.DemoTenantId))
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
            using (AnApplication sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                Enumerable.Range(1, 100).AsParallel().ForAll(i =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    using (sut.BeginScope(new SystemIdentity(), new TenantId(i)))
                    {
                        TenantId insideScopeTenantId = sut.CompositionRoot.GetInstance<ICurrentTHolder<TenantId>>().Current;
                        Assert.True(insideScopeTenantId.HasValue);
                        Assert.Equal(i, insideScopeTenantId.Value);
                    }
                    // ReSharper restore AccessToDisposedClosure
                });
            }
        }

        [Fact]
        public async Task CanConfigureScopeOnInvoke()
        {
            using (AnApplication sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                Enumerable.Range(1, 100).AsParallel().ForAll(i =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    sut.Invoke(() =>
                               {
                                   Assert.Equal(i.ToString(), sut.CompositionRoot.GetInstance<SomeState>().Value);
                               },
                               new SystemIdentity(),
                               new TenantId(1),
                               cr =>
                               {
                                   cr.GetInstance<SomeState>().Value = i.ToString();
                               });
                    // ReSharper restore AccessToDisposedClosure
                });
            }
        }

        [Fact]
        public async Task MaintainsCorrelationWhenBeginningScopes()
        {
            using (AnApplication sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                var usedCorrelationIds = new ConcurrentBag<Guid>();
                Enumerable.Range(1, 100).AsParallel().ForAll(i =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    using (sut.BeginScope(new SystemIdentity(), new TenantId(i)))
                    {
                        Correlation insideScopeCorrelation = sut.CompositionRoot.GetInstance<ICurrentTHolder<Correlation>>().Current;
                        Assert.NotEqual(Guid.Empty, insideScopeCorrelation.Id);
                        Assert.DoesNotContain(insideScopeCorrelation.Id, usedCorrelationIds);
                        usedCorrelationIds.Add(insideScopeCorrelation.Id);
                    }
                    // ReSharper restore AccessToDisposedClosure
                });
            }
        }

        [Fact]
        public async Task MaintainsIdentityWhenBeginningScopes()
        {
            using (AnApplication sut = CreateSystemUnderTest())
            {
                await sut.Boot();
                Enumerable.Range(1, 100).AsParallel().ForAll(i =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    using (sut.BeginScope(new GenericIdentity(i.ToString()), new TenantId(100)))
                    {
                        IIdentity insideScopeIdentity = sut.CompositionRoot.GetInstance<ICurrentTHolder<IIdentity>>().Current;
                        Assert.Equal(i.ToString(), insideScopeIdentity.Name);
                    }
                    // ReSharper restore AccessToDisposedClosure
                });
            }
        }

        private AnApplication CreateSystemUnderTest()
        {
            var compositionRoot = new SimpleInjectorCompositionRoot();
            var sut = new AnApplication(compositionRoot);
            IEventBus eventBus = new InMemoryEventBus(sut);
            sut.CompositionRoot.RegisterModules(
                new InfrastructureModule(new DebugExceptionLogger(), eventBus),
                new ADomainModule(typeof(AnApplication).Assembly, typeof(AggregateRoot).Assembly),
                _persistenceModule);

            return sut;
        }
    }
}