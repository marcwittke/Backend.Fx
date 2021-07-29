using System.Threading;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheMultiTenantApplication
    {
        private readonly IBackendFxApplication _sut;
        private readonly IModule _multiTenancyModule = A.Fake<IModule>();
        private readonly ITenantService _tenantService = A.Fake<ITenantService>();
        private readonly IBackendFxApplication _application = A.Fake<IBackendFxApplication>();

        public TheMultiTenantApplication()
        {
            _sut = new MultiTenantApplication(_application);
        }

        
        [Fact]
        public void DelegatesAllCalls()
        {
            // ReSharper disable once UnusedVariable
            IBackendFxApplicationAsyncInvoker ai = _sut.AsyncInvoker;
            A.CallTo(() => _application.AsyncInvoker).MustHaveHappenedOnceExactly();

            // ReSharper disable once UnusedVariable
            ICompositionRoot cr = _sut.CompositionRoot;
            A.CallTo(() => _application.CompositionRoot).MustHaveHappenedOnceOrMore();

            // ReSharper disable once UnusedVariable
            IBackendFxApplicationInvoker i = _sut.Invoker;
            A.CallTo(() => _application.Invoker).MustHaveHappenedOnceOrMore();

            // ReSharper disable once UnusedVariable
            IMessageBus mb = _sut.MessageBus;
            A.CallTo(() => _application.MessageBus).MustHaveHappenedOnceExactly();

            _sut.BootAsync();
            A.CallTo(() => _application.BootAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            _sut.Dispose();
            A.CallTo(() => _application.Dispose()).MustHaveHappenedOnceExactly();

            _sut.WaitForBoot();
            A.CallTo(() => _application.WaitForBoot(A<int>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        } 
    }
}