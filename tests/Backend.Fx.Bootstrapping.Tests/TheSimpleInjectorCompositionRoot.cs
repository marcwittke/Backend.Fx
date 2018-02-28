namespace Backend.Fx.Bootstrapping.Tests
{
    using System;
    using System.Diagnostics;
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
    using Patterns.IdGeneration;
    using Patterns.UnitOfWork;
    using SimpleInjector;
    using Testing.InMemoryPersistence;
    using Xunit;

    public class TheSimpleInjectorCompositionRoot : IDisposable
    {
        private readonly SimpleInjectorCompositionRoot sut;
        
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

        public TheSimpleInjectorCompositionRoot()
        {

            var tenantManager = A.Fake<ITenantManager>();
            TenantId[] tenantIds = { new TenantId(999) };
            A.CallTo(() => tenantManager.GetTenantIds()).Returns(tenantIds);


            sut = new SimpleInjectorCompositionRoot();
            var domainAssembly = typeof(AnAggregate).GetTypeInfo().Assembly;
            sut.RegisterModules(
                new AnApplicationModule(domainAssembly),
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
            Assert.Null(sut.GetCurrentScopeForTestsOnly());

            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                var sutClock = sut.GetInstance<IClock>();
                var scopeClock = scope.GetInstance<IClock>();
                Assert.NotNull(sutClock);
                Assert.NotNull(scopeClock);
                Assert.Equal(sutClock, scopeClock);
            }


            Assert.Null(sut.GetCurrentScopeForTestsOnly());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IClock>());
            Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));
        }

        [Fact]
        public void BeginsReadonlyUnitOfWork()
        {
            
            Assert.Throws<ActivationException>(() => sut.GetInstance<IUnitOfWork>());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IReadonlyUnitOfWork>());

            var scope = sut.BeginScope(new SystemIdentity(), new TenantId(111));

            scope.BeginUnitOfWork(true);

            using (var uowFake = sut.GetInstance<IReadonlyUnitOfWork>())
            {
                Assert.NotNull(uowFake);
                A.CallTo(() => uowFake.Begin()).MustHaveHappened(Repeated.Exactly.Once);

                uowFake.Complete();

                A.CallTo(() => uowFake.Complete()).MustHaveHappened(Repeated.Exactly.Once);

                scope.Dispose();
                A.CallTo(() => uowFake.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
            }
        }

        [Fact]
        public void BeginsButDoesNotCompleteReadonlyUnitOfWorkOnFailure()
        {
            Assert.Throws<ActivationException>(() => sut.GetInstance<IUnitOfWork>());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IReadonlyUnitOfWork>());

            IUnitOfWork uowFake = null;
            try
            {
                using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(111)))
                {
                    scope.BeginUnitOfWork(true);
                    uowFake = sut.GetInstance<IReadonlyUnitOfWork>();

                    throw new InvalidOperationException("This is the siumulation of an error inside the business transaction");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            A.CallTo(() => uowFake.Complete()).MustNotHaveHappened();
            A.CallTo(() => uowFake.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void BeginsAndCompletesUnitOfWork()
        {
            
            Assert.Throws<ActivationException>(() => sut.GetInstance<IUnitOfWork>());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IReadonlyUnitOfWork>());

            var scope = sut.BeginScope(new SystemIdentity(), new TenantId(111));

            scope.BeginUnitOfWork(false);

            using (var unitOfWork = sut.GetInstance<IUnitOfWork>())
            {
                Assert.NotNull(unitOfWork);
                
                unitOfWork.Complete();
                Assert.Equal(1, ((InMemoryUnitOfWork)unitOfWork).CommitCalls);
                Assert.Equal(0, ((InMemoryUnitOfWork)unitOfWork).RollbackCalls);
            }
        }

        [Fact]
        public void BeginsButDoesNotCompleteUnitOfWorkOnFailure()
        {
            
            Assert.Throws<ActivationException>(() => sut.GetInstance<IUnitOfWork>());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IReadonlyUnitOfWork>());

            IUnitOfWork unitOfWork = null;
            try
            {
                using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(111)))
                {
                    unitOfWork = scope.BeginUnitOfWork(false);
                    throw new InvalidOperationException("This is the siumulation of an error inside the business transaction");
                    //scope.CompleteUnitOfWork();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal(0, ((InMemoryUnitOfWork)unitOfWork).CommitCalls);
            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal(1, ((InMemoryUnitOfWork)unitOfWork).RollbackCalls);
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
