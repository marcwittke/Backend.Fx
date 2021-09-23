using System.Threading;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheBackendFxDbApplication
    {
        private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter
            = A.Fake<IDatabaseAvailabilityAwaiter>();

        private readonly IDatabaseBootstrapper _databaseBootstrapper = A.Fake<IDatabaseBootstrapper>();

        private readonly DiTestFakes _fakes = new();
        private readonly IBackendFxApplication _sut;

        public TheBackendFxDbApplication()
        {
            IBackendFxApplication application = new BackendFxApplication(
                _fakes.CompositionRoot,
                _fakes.MessageBus,
                A.Fake<IExceptionLogger>());
            _sut = new BackendFxDbApplication(_databaseBootstrapper, _databaseAvailabilityAwaiter, application);
        }

        [Fact]
        public void CallsDatabaseBootExtensionPointsOnBoot()
        {
            A.CallTo(() => _databaseAvailabilityAwaiter.WaitForDatabase(A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => _databaseBootstrapper.EnsureDatabaseExistence()).MustNotHaveHappened();
            _sut.BootAsync();
            A.CallTo(() => _databaseAvailabilityAwaiter.WaitForDatabase(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _databaseBootstrapper.EnsureDatabaseExistence()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CallsDatabaseBootstrapperDisposeOnDispose()
        {
            _sut.Dispose();
            A.CallTo(() => _databaseBootstrapper.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DelegatesAllCalls()
        {
            var application = A.Fake<IBackendFxApplication>();
            var sut = new BackendFxDbApplication(
                A.Fake<IDatabaseBootstrapper>(),
                A.Fake<IDatabaseAvailabilityAwaiter>(),
                application);

            // ReSharper disable once UnusedVariable
            var ai = sut.AsyncInvoker;
            A.CallTo(() => application.AsyncInvoker).MustHaveHappenedOnceExactly();

            // ReSharper disable once UnusedVariable
            var cr = sut.CompositionRoot;
            A.CallTo(() => application.CompositionRoot).MustHaveHappenedOnceExactly();

            // ReSharper disable once UnusedVariable
            var i = sut.Invoker;
            A.CallTo(() => application.Invoker).MustHaveHappenedOnceExactly();

            // ReSharper disable once UnusedVariable
            var mb = sut.MessageBus;
            A.CallTo(() => application.MessageBus).MustHaveHappenedOnceExactly();

            sut.BootAsync();
            A.CallTo(() => application.BootAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            sut.Dispose();
            A.CallTo(() => application.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}
