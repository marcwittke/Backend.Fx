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
        private readonly IBackendFxApplicationInvoker _invoker = A.Fake<IBackendFxApplicationInvoker>();

        private readonly AllTenantBackendFxApplicationInvoker _sut;
        private readonly ITenantIdProvider _tenantService = A.Fake<ITenantIdProvider>();

        public TheAllTenantBackendFxApplicationInvoker()
        {
            _sut = new AllTenantBackendFxApplicationInvoker(_tenantService, _invoker);
        }

        [Fact]
        public void InvokesActionForAllTenants()
        {
            TenantId[] demoTenantIds = Enumerable.Range(0, 10).Select(i => new TenantId(i)).ToArray();
            TenantId[] prodTenantIds = Enumerable.Range(10, 10).Select(i => new TenantId(i)).ToArray();
            A.CallTo(() => _tenantService.GetActiveDemonstrationTenantIds()).Returns(demoTenantIds);
            A.CallTo(() => _tenantService.GetActiveProductionTenantIds()).Returns(prodTenantIds);

            _sut.Invoke(_ => { });

            foreach (var tenantId in demoTenantIds)
            {
                A.CallTo(
                        () => _invoker.Invoke(
                            A<Action<IInstanceProvider>>._,
                            A<IIdentity>._,
                            A<TenantId>.That.IsSameAs(tenantId),
                            A<Guid?>._))
                    .MustHaveHappenedOnceExactly();
            }

            foreach (var tenantId in prodTenantIds)
            {
                A.CallTo(
                        () => _invoker.Invoke(
                            A<Action<IInstanceProvider>>._,
                            A<IIdentity>._,
                            A<TenantId>.That.IsSameAs(tenantId),
                            A<Guid?>._))
                    .MustHaveHappenedOnceExactly();
            }
        }
    }
}
