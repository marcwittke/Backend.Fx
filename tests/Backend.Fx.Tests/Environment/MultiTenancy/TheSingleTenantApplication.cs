using System.Threading;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheSingleTenantApplication
    {
        private readonly IBackendFxApplication _sut;
        private readonly IModule _multiTenancyModule = A.Fake<IModule>();
        private readonly ITenantService _tenantService = A.Fake<ITenantService>();
        private readonly TenantCreationParameters _tenantCreationParameters = new TenantCreationParameters("n", "d", false);
        private readonly ICompositionRoot _compositionRoot = A.Fake<ICompositionRoot>();

        public TheSingleTenantApplication()
        {
            var application = A.Fake<IBackendFxApplication>();

            A.CallTo(() => application.CompositionRoot).Returns(_compositionRoot);

            _sut = new SingleTenantApplication(_tenantService,
                                               _multiTenancyModule,
                                               _tenantCreationParameters,
                                               application);
        }

        [Fact]
        public void RegistersMultiTenancyModuleOnBoot()
        {
            _sut.Boot();
            A.CallTo(() => _compositionRoot.RegisterModules(A<IModule>.That.IsEqualTo(_multiTenancyModule))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CreatesTenantOnBootWhenNotExistent()
        {
            _sut.Boot();
            A.CallTo(() => _tenantService.CreateTenant(A<TenantCreationParameters>.That.IsEqualTo(_tenantCreationParameters))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CreatesNoTenantOnBootWhenExistent()
        {
            A.CallTo(() => _tenantService.GetActiveTenantIds()).Returns(new[] {new TenantId(1)});
            _sut.Boot();
            A.CallTo(() => _tenantService.CreateTenant(A<TenantCreationParameters>.That.IsEqualTo(_tenantCreationParameters))).MustNotHaveHappened();
        }

        [Fact]
        public void DelegatesAllCalls()
        {
            var application = A.Fake<IBackendFxApplication>();
            var sut = new SingleTenantApplication(A.Fake<ITenantService>(),
                                                  A.Fake<IModule>(),
                                                  _tenantCreationParameters,
                                                  application);

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

            sut.Boot();
            A.CallTo(() => application.Boot(A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            sut.Dispose();
            A.CallTo(() => application.Dispose()).MustHaveHappenedOnceExactly();

            sut.WaitForBoot();
            A.CallTo(() => application.WaitForBoot(A<int>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}