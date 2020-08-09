using System;
using System.Threading;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
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

        [Fact]
        public void DelegatesAllCalls()
        {
            var application =A.Fake<IBackendFxApplication>();
            var sut = new BackendFxDbApplication(A.Fake<IDatabaseBootstrapper>(),
                                                 A.Fake<IDatabaseAvailabilityAwaiter>(),
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