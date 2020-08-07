using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheBackendFxDbApplication
    {
        private readonly DITestFakes _fakes = new DITestFakes();
        private TestApplication _sut;

        public TheBackendFxDbApplication()
        {
            _sut = new TestApplication(_fakes.CompositionRoot, _fakes.MessageBus, _fakes.InfrastructureModule, _fakes.ExceptionLogger, _fakes.DatabaseBootstrapper);
        }

        [Fact]
        public void CallsDatabaseBootExtensionPointsOnBoot()
        {
            Assert.False(_sut.OnDatabaseBootCalled);
            Assert.False(_sut.OnDatabaseBootedCalled);
            _sut.Boot();
            Assert.True(_sut.OnDatabaseBootCalled);
            Assert.True(_sut.OnDatabaseBootedCalled);
        }
        
        
        private class TestApplication : BackendFxDbApplication
        {
            public TestApplication(ICompositionRoot compositionRoot, IMessageBus messageBus, IInfrastructureModule infrastructureModule, IExceptionLogger exceptionLogger, IDatabaseBootstrapper databaseBootstrapper)
                : base(compositionRoot, messageBus, infrastructureModule, exceptionLogger, databaseBootstrapper)
            {
            }

            public bool OnDatabaseBootCalled { get; set; }
            public bool OnDatabaseBootedCalled { get; set; }

            protected override Task OnDatabaseBoot(CancellationToken cancellationToken)
            {
                OnDatabaseBootCalled = true;
                return base.OnDatabaseBoot(cancellationToken);
            }

            protected override Task OnDatabaseBooted(CancellationToken cancellationToken)
            {
                OnDatabaseBootedCalled = true;
                return base.OnDatabaseBooted(cancellationToken);
            }
        }
    }
}