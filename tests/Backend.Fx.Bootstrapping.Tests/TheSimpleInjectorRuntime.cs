namespace Backend.Fx.Bootstrapping.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using DummyImpl;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using Exceptions;
    using FakeItEasy;
    using NLogLogging;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation;
    using Patterns.Jobs;
    using Patterns.UnitOfWork;
    using SimpleInjector;
    using Xunit;

    public class TheSimpleInjectorRuntime : IDisposable, IClassFixture<NLogLoggingFixture>
    {
        private readonly TestRuntime sut;
        private readonly ITenantManager tenantManager;

        public TheSimpleInjectorRuntime()
        {
            IDatabaseManager databaseManager = A.Fake<IDatabaseManager>();

            tenantManager = A.Fake<ITenantManager>();
            A.CallTo(() => tenantManager.IsActive(A<TenantId>._)).Returns(true);
            TenantId[] tenantIds = {new TenantId(999)};
            A.CallTo(() => tenantManager.GetTenantIds()).Returns(tenantIds);

            sut = new TestRuntime(tenantManager, databaseManager);

        }

        [Fact]
        public void CallsAllRelevantMethodsOnBoot()
        {
            sut.Boot();
            Assert.True(sut.BootApplicationWasCalled);
            Assert.True(sut.BootPersistenceWasCalled);
            Assert.True(sut.InitializeJobSchedulerWasCalled);
        }

        [Fact]
        public void DoesNotAllowBeginningScopeWhenTenantIsDeactivated()
        {
            A.CallTo(() => tenantManager.IsActive(A<TenantId>._)).Returns(false);
            sut.Boot();
            Assert.Throws<UnprocessableException>(() => sut.BeginScope(new SystemIdentity(), new TenantId(111)));
        }

        [Fact]
        public void ProvidesAutoRegisteredDomainServices()
        {
            sut.Boot();
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                Assert.IsType<TestDomainService>(scope.GetInstance<ITestDomainService>());
            }
        }

        [Fact]
        public void ProvidesAutoRegisteredApplicationServices()
        {
            sut.Boot();
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                Assert.IsType<TestApplicationService>(scope.GetInstance<ITestApplicationService>());
            }
        }

        [Fact]
        public void MaintainsTenantIdWhenBeginningScopes()
        {
            sut.Boot();
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
            sut.Boot();
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
            sut.Boot();
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
            sut.Boot();
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
            sut.Boot();
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
            sut.Boot();
            Assert.Null(sut.GetCurrentScopeForTestsOnly());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IClock>());
            Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));


            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                Assert.NotNull(sut.GetInstance<IClock>());
                Assert.NotNull(scope.GetInstance<IClock>());
            }


            Assert.Null(sut.GetCurrentScopeForTestsOnly());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IClock>());
            Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));
        }

        [Fact]
        public void CanInterruptCurrentScopeWithAction()
        {
            sut.Boot();
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                IClock instance1 = scope.GetInstance<IClock>();
                LateResolver<IAmLateResolved> lateResolver = scope.GetInstance<LateResolver<IAmLateResolved>>();
                IAmLateResolved lateResolvedInstance1 = lateResolver.Resolve();

                scope.CompleteCurrentScope_InvokeAction_BeginNewScope(() =>
                {
                    Assert.Null(sut.GetCurrentScopeForTestsOnly());
                    Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));
                });

                IClock instance2 = scope.GetInstance<IClock>();
                Assert.NotEqual(instance1, instance2);

                IAmLateResolved lateResolvedInstance2 = lateResolver.Resolve();
                Assert.NotEqual(lateResolvedInstance1, lateResolvedInstance2);
            }
        }

        [Fact]
        public void CanInterruptCurrentScopeWithFunc()
        {
            sut.Boot();
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                IClock instance1 = scope.GetInstance<IClock>();
                LateResolver<IAmLateResolved> lateResolver = scope.GetInstance<LateResolver<IAmLateResolved>>();
                IAmLateResolved lateResolvedInstance1 = lateResolver.Resolve();

                int i = scope.CompleteCurrentScope_InvokeFunction_BeginNewScope(() =>
                                                                      {
                                                                          Assert.Null(sut.GetCurrentScopeForTestsOnly());
                                                                          Assert.Throws<ActivationException>(() => sut.GetInstance(typeof(IClock)));
                                                                          return 42;
                                                                      });

                Assert.Equal(42, i);
                IClock instance2 = scope.GetInstance<IClock>();
                Assert.NotEqual(instance1, instance2);

                IAmLateResolved lateResolvedInstance2 = lateResolver.Resolve();
                Assert.NotEqual(lateResolvedInstance1, lateResolvedInstance2);
            }
        }

        [Fact]
        public void BeginsAndCompletesReadonlyUnitOfWork()
        {
            sut.Boot();
            Assert.Throws<ActivationException>(() => sut.GetInstance<IUnitOfWork>());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IReadonlyUnitOfWork>());

            var scope = sut.BeginScope(new SystemIdentity(), new TenantId(111));

            scope.BeginUnitOfWork(true);

            var uowFake = sut.GetInstance<IReadonlyUnitOfWork>();
            Assert.NotNull(uowFake);
            A.CallTo(() => uowFake.Begin()).MustHaveHappened(Repeated.Exactly.Once);

            scope.CompleteUnitOfWork();

            A.CallTo(() => uowFake.Complete()).MustHaveHappened(Repeated.Exactly.Once);

            scope.Dispose();
            A.CallTo(() => uowFake.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void BeginsButDoesNotCompleteReadonlyUnitOfWorkOnFailure()
        {
            sut.Boot();
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
                    //scope.CompleteUnitOfWork();
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
            sut.Boot();
            Assert.Throws<ActivationException>(() => sut.GetInstance<IUnitOfWork>());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IReadonlyUnitOfWork>());

            var scope = sut.BeginScope(new SystemIdentity(), new TenantId(111));

            scope.BeginUnitOfWork(false);

            var uowFake = sut.GetInstance<IUnitOfWork>();
            Assert.NotNull(uowFake);
            A.CallTo(() => uowFake.Begin()).MustHaveHappened(Repeated.Exactly.Once);

            scope.CompleteUnitOfWork();

            A.CallTo(() => uowFake.Complete()).MustHaveHappened(Repeated.Exactly.Once);

            scope.Dispose();
            A.CallTo(() => uowFake.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void BeginsButDoesNotCompleteUnitOfWorkOnFailure()
        {
            sut.Boot();
            Assert.Throws<ActivationException>(() => sut.GetInstance<IUnitOfWork>());
            Assert.Throws<ActivationException>(() => sut.GetInstance<IReadonlyUnitOfWork>());

            IUnitOfWork uowFake = null;
            try
            {
                using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(111)))
                {
                    scope.BeginUnitOfWork(true);
                    uowFake = sut.GetInstance<IUnitOfWork>();

                    throw new InvalidOperationException("This is the siumulation of an error inside the business transaction");
                    //scope.CompleteUnitOfWork();
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
        public void CanProvideEventHandlers()
        {
            sut.Boot();

            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(1)))
            {
                var handlers = sut.GetAllEventHandlers<ADomainEvent>().ToArray();
                
                // these three handlers should have been auto registered during boot by scanning the assembly
                Assert.True(handlers.OfType<ADomainEventHandler1>().Any());
                Assert.True(handlers.OfType<ADomainEventHandler2>().Any());
                Assert.True(handlers.OfType<ADomainEventHandler3>().Any());
            }
        }

        [Fact]
        public async void CanExecuteJobs()
        {
            var someJob = A.Fake<IJob>();
            sut.Boot(container =>
            {
                container.Register(()=>someJob);

            });
            await sut.ExecuteJobAsync<IJob>();

            A.CallTo(() => someJob.Execute()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void CanExecuteJobsWithDelay()
        {
            var someJob = A.Fake<IJob>();
            sut.Boot(container => container.Register(() => someJob));
            await sut.ExecuteJobAsync<IJob>(null, 1);

            A.CallTo(() => someJob.Execute()).MustHaveHappened(Repeated.Exactly.Once);
        }

        public void Dispose()
        {
            sut.Dispose();
        }
    }
}
