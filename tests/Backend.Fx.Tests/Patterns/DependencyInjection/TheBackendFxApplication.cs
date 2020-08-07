using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheBackendFxApplication
    {
        private readonly TestApplication _sut;
        private readonly DITestFakes _fakes;

        public TheBackendFxApplication()
        {
            _fakes = new DITestFakes();
            _sut = new TestApplication(_fakes.CompositionRoot, _fakes.MessageBus, _fakes.InfrastructureModule, _fakes.ExceptionLogger);
        }

        [Fact]
        public void ProvidesInvokerForMessageBus()
        {
            A.CallTo(() => _fakes.MessageBus.ProvideInvoker(A<IBackendFxApplicationInvoker>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RegistersInfrastructureModule()
        {
            A.CallTo(() => _fakes.InfrastructureModule.RegisterCorrelationHolder<CurrentCorrelationHolder>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.InfrastructureModule.RegisterDomainEventAggregator(A<Func<IDomainEventAggregator>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.InfrastructureModule.RegisterIdentityHolder<CurrentIdentityHolder>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.InfrastructureModule.RegisterMessageBusScope(A<Func<IMessageBusScope>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.InfrastructureModule.RegisterTenantHolder<CurrentTenantIdHolder>()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void VerifiesCompositionRootOnBoot()
        {
            _sut.Boot();
            A.CallTo(() => _fakes.CompositionRoot.Verify()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanWaitForBoot()
        {
            var sw = new Stopwatch();
            _sut.BootDuration = 200;

            Task.Factory.StartNew(() => _sut.Boot());
            sw.Start();
            Assert.True(_sut.WaitForBoot());
            Assert.True(sw.ElapsedMilliseconds >= _sut.BootDuration);
        }

        [Fact]
        public void CallsBootExtensionPointsOnBoot()
        {
            Assert.False(_sut.OnBootCalled);
            Assert.False(_sut.OnBootedCalled);
            _sut.Boot();
            Assert.True(_sut.OnBootCalled);
            Assert.True(_sut.OnBootedCalled);
        }

        [Fact]
        public void DisposesCompositionRootOnDispose()
        {
            _sut.Boot();
            _sut.Dispose();
            A.CallTo(() => _fakes.CompositionRoot.Dispose()).MustHaveHappenedOnceExactly();
        }
        
        private class TestApplication : BackendFxApplication
        {
            public int BootDuration { get; set; }

            public TestApplication(ICompositionRoot compositionRoot, IMessageBus messageBus, IInfrastructureModule infrastructureModule, IExceptionLogger exceptionLogger)
                : base(compositionRoot, messageBus, infrastructureModule, exceptionLogger)
            {
            }

            public bool OnBootCalled { get; set; }
            public bool OnBootedCalled { get; set; }

            protected override Task OnBoot(CancellationToken cancellationToken)
            {
                OnBootCalled = true;
                Thread.Sleep(BootDuration);
                return base.OnBoot(cancellationToken);
            }

            protected override Task OnBooted(CancellationToken cancellationToken)
            {
                OnBootedCalled = true;
                return base.OnBooted(cancellationToken);
            }
        }
    }
}