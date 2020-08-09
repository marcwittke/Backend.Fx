using System;
using System.Linq;
using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheAllTenantBackendFxApplicationInvoker
    {
        public TheAllTenantBackendFxApplicationInvoker()
        {
            _sut = new AllTenantBackendFxApplicationInvoker(_tenantService, _invoker);
        }

        private readonly AllTenantBackendFxApplicationInvoker _sut;
        private readonly ITenantService _tenantService = A.Fake<ITenantService>();
        private readonly IBackendFxApplicationInvoker _invoker = A.Fake<IBackendFxApplicationInvoker>();

        [Fact]
        public void InvokesActionForAllTenants()
        {
            var tenantIds = Enumerable.Range(0, 10).Select(i => new TenantId(i)).ToArray();
            A.CallTo(() => _tenantService.GetActiveTenantIds()).Returns(tenantIds);

            _sut.Invoke(_ => { });

            foreach (TenantId tenantId in tenantIds)
                A.CallTo(() => _invoker.Invoke(A<Action<IInstanceProvider>>._, A<IIdentity>._, A<TenantId>.That.IsSameAs(tenantId), A<Guid?>._))
                 .MustHaveHappenedOnceExactly();
        }
    }
}