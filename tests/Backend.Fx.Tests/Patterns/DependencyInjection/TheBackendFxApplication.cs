using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheBackendFxApplication : TestWithLogging
    {
        private readonly IBackendFxApplication _sut;
        private readonly DiTestFakes _fakes = new DiTestFakes();

        public TheBackendFxApplication(ITestOutputHelper output) : base(output)
        {
            _sut = new BackendFxApplication(_fakes.CompositionRoot, A.Fake<IExceptionLogger>());
        }

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
        public void ProvidesDomainEventAggregator()
        {
            using (IServiceScope scope = _sut.CompositionRoot.BeginScope())
            {
                var domainEventAggregator = scope.ServiceProvider.GetService<IDomainEventAggregator>();
                Assert.NotNull(domainEventAggregator);
            }

            A.CallTo(() => _fakes.ServiceProvider.GetService(A<Type>.That.IsEqualTo(typeof(IDomainEventAggregator))))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ProvidesExceptionLoggingInvoker()
        {
            Assert.IsType<ExceptionLoggingInvoker>(_sut.Invoker);
        }


        [Fact]
        public void VerifiesCompositionRootOnBoot()
        {
            _sut.BootAsync();
            A.CallTo(() => _fakes.CompositionRoot.Verify()).MustHaveHappenedOnceExactly();
        }
    }
}