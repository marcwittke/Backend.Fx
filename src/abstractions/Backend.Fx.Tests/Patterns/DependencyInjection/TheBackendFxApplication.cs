using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        public TheBackendFxApplication()
        {
            _fakes = new DiTestFakes();

            Func<IDomainEventAggregator> domainEventAggregatorFactory = () => null;
            A.CallTo(() => _fakes.InstanceProvider.GetInstance<IDomainEventAggregator>()).ReturnsLazily(() => domainEventAggregatorFactory.Invoke());

            Func<IMessageBusScope> messageBusScopeFactory = () => null;
            A.CallTo(() => _fakes.InstanceProvider.GetInstance<IMessageBusScope>()).ReturnsLazily(() => messageBusScopeFactory.Invoke());

            _sut = new BackendFxApplication(_fakes.CompositionRoot, _fakes.MessageBus, A.Fake<IExceptionLogger>());
        }

        private readonly IBackendFxApplication _sut;
        private readonly DiTestFakes _fakes;

        
        [Fact]
        public void CanWaitForBoot()
        {
            int bootTime = 200;
            A.CallTo(() => _fakes.CompositionRoot.Verify()).Invokes(() => Thread.Sleep(bootTime));
            var sw = new Stopwatch();
            
            Task.Factory.StartNew(() => _sut.BootAsync());
            sw.Start();
            Assert.True(_sut.WaitForBoot());
            Assert.True(sw.ElapsedMilliseconds >= bootTime);
        }
        

        [Fact]
        public void DisposesCompositionRootOnDispose()
        {
            _sut.BootAsync();
            _sut.Dispose();
            A.CallTo(() => _fakes.CompositionRoot.Dispose()).MustHaveHappenedOnceExactly();
        }
        

        [Fact]
        public void ProvidesExceptionLoggingAsyncInvoker()
        {
            Assert.IsType<ExceptionLoggingAsyncInvoker>(_sut.AsyncInvoker);
        }
        
        
        
        [Fact]
        public void ProvidesExceptionLoggingInvoker()
        {
            Assert.IsType<ExceptionLoggingInvoker>(_sut.Invoker);
        }
        

        [Fact]
        public void IntegratesWithMessageBus()
        {
            A.CallTo(() => _fakes.MessageBus.ProvideInvoker(A<IBackendFxApplicationInvoker>._)).MustHaveHappenedOnceExactly();
        }

        
        [Fact]
        public void VerifiesCompositionRootOnBoot()
        {
            _sut.BootAsync();
            A.CallTo(() => _fakes.CompositionRoot.Verify()).MustHaveHappenedOnceExactly();
        }
    }
}