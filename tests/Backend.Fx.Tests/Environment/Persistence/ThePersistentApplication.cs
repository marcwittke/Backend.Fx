using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Extensions.Persistence;
using Backend.Fx.Logging;
using Backend.Fx.Tests.Patterns.DependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.Persistence
{
    public class ThePersistentApplication : TestWithLogging
    {
        public ThePersistentApplication(ITestOutputHelper output): base(output)
        {
            IBackendFxApplication application = new BackendFxApplication(_fakes.CompositionRoot, A.Fake<IExceptionLogger>());
            _sut = new PersistentApplication(
                _databaseBootstrapper,
                _databaseAvailabilityAwaiter,
                A.Fake<IModule>(),
                application);
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
            _sut.BootAsync();
            A.CallTo(() => _databaseAvailabilityAwaiter.WaitForDatabase(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _databaseBootstrapper.EnsureDatabaseExistence()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task DelegatesAllCalls()
        {
            var application =A.Fake<IBackendFxApplication>();
            var sut = new PersistentApplication(A.Fake<IDatabaseBootstrapper>(),
                A.Fake<IDatabaseAvailabilityAwaiter>(),
                A.Fake<IModule>(),
                application);
            
            Fake.ClearRecordedCalls(application);

            object unused1 = sut.AsyncInvoker;
            A.CallTo(() => application.AsyncInvoker).MustHaveHappenedOnceExactly();

            object unused2 = sut.CompositionRoot;
            A.CallTo(() => application.CompositionRoot).MustHaveHappenedOnceExactly();

            object unused3 = sut.Invoker;
            A.CallTo(() => application.Invoker).MustHaveHappenedOnceExactly();

            await sut.BootAsync();
            A.CallTo(() => application.BootAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();

            sut.Dispose();
            A.CallTo(() => application.Dispose()).MustHaveHappenedOnceExactly();

            sut.WaitForBoot();
            A.CallTo(() => application.WaitForBoot(A<int>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
        
        
    }
}