namespace Backend.Fx.Tests.Patterns.Jobs
{
    using System.Security.Principal;
    using FakeItEasy;
    using Fx.Environment.MultiTenancy;
    using Fx.Patterns.DependencyInjection;
    using Fx.Patterns.Jobs;
    using JetBrains.Annotations;
    using Xunit;

    public class TheJobExecutor
    {
        private readonly IJobExecutor sut;
        private readonly IScopeManager scopeManager;
        
        public TheJobExecutor()
        {
            MyJob.ExecutionCounter = 0;

            var tenantManager = A.Fake<ITenantManager>();
            A.CallTo(() => tenantManager.GetTenantIds()).Returns(new[] {
                new TenantId(1),
                new TenantId(2),
                new TenantId(3),
                new TenantId(4)
            });

            scopeManager = A.Fake<IScopeManager>();
            A.CallTo(() => scopeManager.BeginScope(A<IIdentity>._, A<TenantId>._)).Returns(A.Fake<IScope>());

            sut = new JobExecutor(tenantManager, scopeManager);
        }

        [Fact]
        public async void ExecutesTheJobInSystemScopeForEachTenant()
        {
            await sut.ExecuteJobAsync<MyJob>();

            A.CallTo(()=>scopeManager.BeginScope(A<IIdentity>.That.Matches(id => id.Name == "SYSTEM"), A<TenantId>.That.Matches(t => t.Value == 1)))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => scopeManager.BeginScope(A<IIdentity>.That.Matches(id => id.Name == "SYSTEM"), A<TenantId>.That.Matches(t => t.Value == 2)))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => scopeManager.BeginScope(A<IIdentity>.That.Matches(id => id.Name == "SYSTEM"), A<TenantId>.That.Matches(t => t.Value == 3)))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => scopeManager.BeginScope(A<IIdentity>.That.Matches(id => id.Name == "SYSTEM"), A<TenantId>.That.Matches(t => t.Value == 4)))
             .MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(MyJob.ExecutionCounter, 4);
        }

        [Fact]
        public async void ExecutesTheJobInSystemScopeForGivenTenant()
        {
            await sut.ExecuteJobAsync<MyJob>(1);

            A.CallTo(() => scopeManager.BeginScope(A<IIdentity>.That.Matches(id => id.Name == "SYSTEM"), A<TenantId>.That.Matches(t => t.Value == 1)))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => scopeManager.BeginScope(A<IIdentity>.That.Matches(id => id.Name == "SYSTEM"), A<TenantId>.That.Matches(t => t.Value == 2)))
             .MustNotHaveHappened();

            A.CallTo(() => scopeManager.BeginScope(A<IIdentity>.That.Matches(id => id.Name == "SYSTEM"), A<TenantId>.That.Matches(t => t.Value == 3)))
             .MustNotHaveHappened();

            A.CallTo(() => scopeManager.BeginScope(A<IIdentity>.That.Matches(id => id.Name == "SYSTEM"), A<TenantId>.That.Matches(t => t.Value == 4)))
             .MustNotHaveHappened();

            Assert.Equal(MyJob.ExecutionCounter, 1);
        }

        [UsedImplicitly]
        private class MyJob : IJob
        {
            public static int ExecutionCounter = 0;
            public void Execute()
            {
                ExecutionCounter++;
            }
        }
    }
}
