namespace Backend.Fx.Bootstrapping.Tests
{
    using System.Linq;
    using System.Security.Principal;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using FakeItEasy;
    using Patterns.DependencyInjection;
    using SimpleInjector;
    using Xunit;

    public class TheSimpleInjectorRuntime
    {
        private TestRuntime sut;

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
    }
}
