using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Tests.Patterns.Authorization;
using Backend.Fx.TestUtil;
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
        private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

        public TheBackendFxApplication(ITestOutputHelper output) : base(output)
        {
            _sut = new BackendFxApplication(_fakes.CompositionRoot, _exceptionLogger);
        }

        [Fact]
        public void ProvidesSpecificDecoratorWhenPresent()
        {
            var sut =
                new SingleTenantApplication(
                    A.Fake<ITenantRepository>(),
                    true,
                    new MessageBusApplication(
                        A.Fake<IMessageBus>(),
                        new AuthorizingApplication(
                            new BackendFxApplication(
                                CompositionRootType.Microsoft.Create(),
                                new ExceptionLoggers(),
                                typeof(TheAuthorizingApplication).Assembly))));

            var authorizingApplication = sut.As<AuthorizingApplication>();
            Assert.NotNull(authorizingApplication);
        }

        [Fact]
        public void ProvidesNoDecoratorWhenNotPresent()
        {
            var sut =
                new SingleTenantApplication(
                    A.Fake<ITenantRepository>(),
                    true,
                    new MessageBusApplication(
                        A.Fake<IMessageBus>(),
                        new BackendFxApplication(
                            CompositionRootType.Microsoft.Create(),
                            new ExceptionLoggers(),
                            typeof(TheAuthorizingApplication).Assembly)));

            var authorizingApplication = sut.As<AuthorizingApplication>();
            Assert.Null(authorizingApplication);
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
        public async Task DisposesCompositionRootOnDispose()
        {
            await _sut.BootAsync();
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
        public async Task VerifiesCompositionRootOnBoot()
        {
            await _sut.BootAsync();
            A.CallTo(() => _fakes.CompositionRoot.Verify()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LogsButDoesNotHandleExceptions()
        {
            var exception = new Exception();

            await _sut.BootAsync();
            Assert.Throws<Exception>(() =>
                _sut.Invoker.Invoke(sp => throw exception, new AnonymousIdentity(), new TenantId(111)));

            A.CallTo(() => _exceptionLogger.LogException(A<Exception>.That.IsEqualTo(exception)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LogsButDoesNotHandleExceptionsAsync()
        {
            var exception = new Exception();

            await _sut.BootAsync();
            await Assert.ThrowsAsync<Exception>(() =>
                _sut.AsyncInvoker.InvokeAsync(sp => throw exception, new AnonymousIdentity(), new TenantId(111)));

            A.CallTo(() => _exceptionLogger.LogException(A<Exception>.That.IsEqualTo(exception)))
                .MustHaveHappenedOnceExactly();
        }
    }
}