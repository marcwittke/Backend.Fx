namespace Backend.Fx.Bootstrapping.Tests
{
    using System.Linq;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using FakeItEasy;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation;
    using SimpleInjector;
    using Xunit;

    public class TheSimpleInjectorRuntime
    {
        private readonly TestRuntime sut;

        public TheSimpleInjectorRuntime()
        {
            IDatabaseManager databaseManager = A.Fake<IDatabaseManager>();

            ITenantManager tenantManager = A.Fake<ITenantManager>();
            A.CallTo(() => tenantManager.IsActive(A<TenantId>._)).Returns(true);

            sut = new TestRuntime(tenantManager, databaseManager);
            sut.Boot();
        }

        [Fact]
        public void CallsAllRelevantMethodsOnBoot()
        {
            Assert.True(sut.BootApplicationWasCalled);
            Assert.True(sut.BootPersistenceWasCalled);
            Assert.True(sut.InitializeJobSchedulerWasCalled);
        }

        [Fact]
        public void ProvidesAutoRegisteredDomainServices()
        {
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                Assert.IsType<TestDomainService>(scope.GetInstance<ITestDomainService>());
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
            Assert.Throws<ActivationException>(() => sut.GetInstance<ICurrentTHolder<TenantId>>());

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
            Assert.Throws<ActivationException>(() => sut.GetInstance<ICurrentTHolder<IIdentity>>());

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
        public void MaintainsScopeInterruptorWhenBeginningScopes()
        {
            Assert.Throws<ActivationException>(() => sut.GetInstance<ICurrentTHolder<IScopeInterruptor>>());
            
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                IScopeInterruptor insideScopeInterruptor = scope.GetInstance<ICurrentTHolder<IScopeInterruptor>>().Current;
                Assert.Equal(scope, insideScopeInterruptor);
            }
        }

        [Fact]
        public void ProvidesScopedInstancesWhenScopeHasBeenStarted()
        {
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                var currentScope = sut.GetCurrentScopeForTestsOnly();
                Assert.NotNull(currentScope);
                Assert.NotNull(scope.GetInstance<IClock>());
            }
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
                        singletonInstances[indexClosure] = scope.GetInstance<IEventAggregator>();
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
            Assert.Null(sut.GetCurrentScopeForTestsOnly());
            Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));


            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                Assert.NotNull(scope.GetInstance<IClock>());
            }


            Assert.Null(sut.GetCurrentScopeForTestsOnly());
            Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));
        }

        [Fact]
        public void CanInterruptCurrentScope()
        {
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                IClock instance1 = scope.GetInstance<IClock>();
                LateResolver<IAmLateResolved> lateResolver = scope.GetInstance<LateResolver<IAmLateResolved>>();
                IAmLateResolved lateResolvedInstance1 = lateResolver.Resolve();

                scope.CompleteCurrentScope_InvokeAction_BeginNewScope(() => {
                    Assert.Null(sut.GetCurrentScopeForTestsOnly());
                    Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));
                });

                IClock instance2 = scope.GetInstance<IClock>();
                Assert.NotEqual(instance1, instance2);

                IAmLateResolved lateResolvedInstance2 = lateResolver.Resolve();
                Assert.NotEqual(lateResolvedInstance1, lateResolvedInstance2);
            }
        }
    }
}
