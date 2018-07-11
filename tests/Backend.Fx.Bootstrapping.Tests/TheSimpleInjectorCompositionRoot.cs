namespace Backend.Fx.Bootstrapping.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using DummyImpl;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using FakeItEasy;
    using Modules;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation.Integration;
    using Patterns.IdGeneration;
    using SimpleInjector;
    using Testing.InMemoryPersistence;
    using Xunit;

    public class TheSimpleInjectorCompositionRoot : IDisposable
    {
        private readonly SimpleInjectorCompositionRoot sut;
        
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

        public TheSimpleInjectorCompositionRoot()
        {

            var tenantManager = A.Fake<ITenantManager>();
            TenantId[] tenantIds = { new TenantId(999) };
            A.CallTo(() => tenantManager.GetTenantIds()).Returns(tenantIds);


            sut = new SimpleInjectorCompositionRoot();
            var domainAssembly = typeof(AnAggregate).GetTypeInfo().Assembly;
            sut.RegisterModules(
                new ADomainModule(domainAssembly),
                new InMemoryIdGeneratorsModule(),
                new InMemoryPersistenceModule(domainAssembly));
            
            sut.Verify();

        }

        [Fact]
        public void ProvidesAutoRegisteredDomainServices()
        {
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                var testDomainService = scope.GetInstance<ITestDomainService>();
                Assert.IsType<TestDomainService>(testDomainService);
            }
        }

        [Fact]
        public void ProvidesAutoRegisteredDomainServicesThatImplementTwoInterfaces()
        {
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                var testDomainService = scope.GetInstance<ITestDomainService>();
                Assert.IsType<TestDomainService>(testDomainService);

                var anotherTestDomainService = scope.GetInstance<IAnotherTestDomainService>();
                Assert.IsType<TestDomainService>(anotherTestDomainService);

                Assert.True(Equals(testDomainService, anotherTestDomainService));
            }
        }

        [Fact]
        public void ProvidesAutoRegisteredApplicationServices()
        {
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                Assert.IsType<TestApplicationService>(scope.GetInstance<ITestApplicationService>());
            }
        }

        [Fact]
        public void MaintainsTenantIdWhenBeginningScopes()
        {
            Enumerable.Range(1, 100).AsParallel().ForAll(i =>
            {
                using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(i)))
                {
                    var insideScopeTenantId = scope.GetInstance<ICurrentTHolder<TenantId>>().Current;
                    Assert.True(insideScopeTenantId.HasValue);
                    Assert.Equal(i, insideScopeTenantId.Value);
                }
            });
        }

        [Fact]
        public void MaintainsIdentityWhenBeginningScopes()
        {
            Enumerable.Range(1, 100).AsParallel().ForAll(i =>
            {
                using (var scope = sut.BeginScope(new GenericIdentity(i.ToString()), new TenantId(100)))
                {
                    var insideScopeIdentity = scope.GetInstance<ICurrentTHolder<IIdentity>>().Current;
                    Assert.Equal(i.ToString(), insideScopeIdentity.Name);
                }
            });
        }

        [Fact]
        public void ProvidesScopedInstancesWhenScopeHasBeenStarted()
        {
            IClock scope1Clock;
            IClock scope2Clock;

            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                scope1Clock = scope.GetInstance<IClock>();
                Assert.NotNull(scope1Clock);
            }

            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                scope2Clock = scope.GetInstance<IClock>();
                Assert.NotNull(scope2Clock);
            }

            Assert.NotEqual(scope1Clock, scope2Clock);
        }

        [Fact]
        public void ProvidesSingletonAndScopedInstancesAccordingly()
        {
            
            const int parallelScopeCount = 1000;
            object[] scopedInstances = new object[parallelScopeCount];
            object[] singletonInstances = new object[parallelScopeCount];
            Task[] tasks = new Task[parallelScopeCount];

            ManualResetEvent waiter = new ManualResetEvent(false);

            // resolving a singleton service and a scoped service in a massive parallel scenario
            for (int index = 0; index < parallelScopeCount; index++)
            {
                var indexClosure = index;
                tasks[index] = Task.Factory.StartNew(() =>
                {
                    // using the reset event to enforce a maximum grade of parallelism
                    waiter.WaitOne();
                    using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
                    {
                        scopedInstances[indexClosure] = scope.GetInstance<IClock>();
                        singletonInstances[indexClosure] = scope.GetInstance<IEntityIdGenerator>();
                    }
                });
            }

            // let the show begin...
            waiter.Set();
            Task.WaitAll(tasks);

            // asserting for equality: singleton instances must be equal, scoped instances must be unique
            for (int index = 0; index < parallelScopeCount; index++)
            {
                Assert.NotNull(scopedInstances[index]);
                Assert.NotNull(singletonInstances[index]);

                for (int indexComp = 0; indexComp < parallelScopeCount; indexComp++)
                {
                    if (index != indexComp)
                    {
                        Assert.NotEqual(scopedInstances[index], scopedInstances[indexComp]);
                    }

                    Assert.Equal(singletonInstances[index], singletonInstances[indexComp]);
                }
            }
        }

        [Fact]
        public void ThrowsWhenScopedInstanceIsRequestedOutsideScope()
        {
            Assert.Throws<ActivationException>(() => sut.GetInstance<IClock>());
            Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));
            Assert.Null(sut.GetCurrentScope());

            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                var sutClock = sut.GetInstance<IClock>();
                var scopeClock = scope.GetInstance<IClock>();
                Assert.NotNull(sutClock);
                Assert.NotNull(scopeClock);
                Assert.Equal(sutClock, scopeClock);
            }

            Assert.Null(sut.GetCurrentScope());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IClock>());
            Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));
        }

        [Fact]
        public void CanProvideEventHandlers()
        {
            using (sut.BeginScope(new SystemIdentity(), new TenantId(1)))
            {
                var handlers = sut.GetAllEventHandlers<ADomainEvent>().ToArray();
                
                // these three handlers should have been auto registered during boot by scanning the assembly
                Assert.True(handlers.OfType<ADomainEventHandler1>().Any());
                Assert.True(handlers.OfType<ADomainEventHandler2>().Any());
                Assert.True(handlers.OfType<ADomainEventHandler3>().Any());
            }
        }
        
        public void Dispose()
        {
            sut.Dispose();
        }
    }
}
