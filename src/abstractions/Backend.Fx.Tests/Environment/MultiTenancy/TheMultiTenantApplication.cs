using System.Threading;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheMultiTenantApplication
    {
        private readonly IBackendFxApplication _application = A.Fake<IBackendFxApplication>();
        private readonly IBackendFxApplication _sut;

        public TheMultiTenantApplication()
        {
            _sut = new MultiTenantApplication(_application);
        }

        [Fact]
        public void DelegatesAllCalls()
        {
            // ReSharper disable once UnusedVariable
            var ai = _sut.AsyncInvoker;
            A.CallTo(() => _application.AsyncInvoker).MustHaveHappenedOnceExactly();

            // ReSharper disable once UnusedVariable
            var cr = _sut.CompositionRoot;
            A.CallTo(() => _application.CompositionRoot).MustHaveHappenedOnceOrMore();

            // ReSharper disable once UnusedVariable
            var i = _sut.Invoker;
            A.CallTo(() => _application.Invoker).MustHaveHappenedOnceOrMore();

            // ReSharper disable once UnusedVariable
            var mb = _sut.MessageBus;
            A.CallTo(() => _application.MessageBus).MustHaveHappenedOnceExactly();

            _sut.BootAsync();
            A.CallTo(() => _application.BootAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            _sut.Dispose();
            A.CallTo(() => _application.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
