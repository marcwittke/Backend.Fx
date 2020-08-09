using System.Threading;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheBackendFxDbApplication
    {
        public TheBackendFxDbApplication()
        {
            IBackendFxApplication application = new BackendFxApplication(_fakes.CompositionRoot, _fakes.MessageBus, _fakes.InfrastructureModule, _fakes.ExceptionLogger);
            _sut = new BackendFxDbApplication(_databaseBootstrapper, _databaseAvailabilityAwaiter, application);
        }

        private readonly DiTestFakes _fakes = new DiTestFakes();
        private readonly IBackendFxApplication _sut;
        private readonly IDatabaseBootstrapper _databaseBootstrapper = A.Fake<IDatabaseBootstrapper>();
        private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter = A.Fake<IDatabaseAvailabilityAwaiter>();

        [Fact]
        public void CallsDatabaseBootExtensionPointsOnBoot()
        {
            A.CallTo(() => _databaseAvailabilityAwaiter.WaitForDatabase(A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => _databaseBootstrapper.EnsureDatabaseExistence()).MustNotHaveHappened();
            _sut.Boot();
            A.CallTo(() => _databaseAvailabilityAwaiter.WaitForDatabase(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _databaseBootstrapper.EnsureDatabaseExistence()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CallsDatabaseBootstrapperDisposeOnDispose()
        {
            _sut.Dispose();
            A.CallTo(() => _databaseBootstrapper.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}