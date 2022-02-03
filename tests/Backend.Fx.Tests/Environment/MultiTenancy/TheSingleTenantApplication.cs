using System.Threading;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Hacking;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheSingleTenantApplication : TestWithLogging
    {
        private readonly IBackendFxApplication _sut;
        private readonly ITenantService _tenantService = A.Fake<ITenantService>();
        private readonly ICompositionRoot _compositionRoot = A.Fake<ICompositionRoot>();

        public TheSingleTenantApplication(ITestOutputHelper output): base(output)
        {
            var application = A.Fake<IBackendFxApplication>();

            A.CallTo(() => application.CompositionRoot).Returns(_compositionRoot);

            _sut = new SingleTenantApplication(false, _tenantService, application);
        }

        [Fact]
        public void CreatesTenantOnBootWhenNotExistent()
        {
            _sut.BootAsync();
            A.CallTo(() => _tenantService.CreateTenant(A<string>._, A<string>._, A<bool>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CreatesNoTenantOnBootWhenExistent()
        {
            var tenant = new Tenant("single tenant", "", false);
            tenant.SetPrivate(t => t.Id, 1);
            
            A.CallTo(() => _tenantService.GetActiveTenants()).Returns(new[] {tenant});
            _sut.BootAsync();
            A.CallTo(() => _tenantService.CreateTenant(A<string>._, A<string>._, A<bool>._, A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public void DelegatesAllCalls()
        {
            var application = A.Fake<IBackendFxApplication>();
            var sut = new SingleTenantApplication(false, A.Fake<ITenantService>(), application);

            Fake.ClearRecordedCalls(application);
            // ReSharper disable once UnusedVariable
            IBackendFxApplicationAsyncInvoker ai = sut.AsyncInvoker;
            A.CallTo(() => application.AsyncInvoker).MustHaveHappenedOnceExactly();

            // ReSharper disable once UnusedVariable
            ICompositionRoot cr = sut.CompositionRoot;
            A.CallTo(() => application.CompositionRoot).MustHaveHappenedOnceExactly();

            // ReSharper disable once UnusedVariable
            IBackendFxApplicationInvoker i = sut.Invoker;
            A.CallTo(() => application.Invoker).MustHaveHappenedOnceExactly();

            // ReSharper disable once UnusedVariable
            IMessageBus mb = sut.MessageBus;
            A.CallTo(() => application.MessageBus).MustHaveHappenedOnceExactly();

            sut.BootAsync();
            A.CallTo(() => application.BootAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            sut.Dispose();
            A.CallTo(() => application.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}