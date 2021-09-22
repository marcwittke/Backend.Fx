using System;
using System.Diagnostics;
using System.Security.Principal;
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
        private readonly DiTestFakes _fakes;

        private readonly IBackendFxApplication _sut;

        public TheBackendFxApplication()
        {
            _fakes = new DiTestFakes();

            Func<IDomainEventAggregator> domainEventAggregatorFactory = () => null;
            A.CallTo(() => _fakes.InfrastructureModule.RegisterScoped(A<Func<IDomainEventAggregator>>._))
                .Invokes((Func<IDomainEventAggregator> f) => domainEventAggregatorFactory = f);
            A.CallTo(() => _fakes.InstanceProvider.GetInstance<IDomainEventAggregator>())
                .ReturnsLazily(() => domainEventAggregatorFactory.Invoke());

            Func<IMessageBusScope> messageBusScopeFactory = () => null;
            A.CallTo(() => _fakes.InfrastructureModule.RegisterScoped(A<Func<IMessageBusScope>>._))
                .Invokes((Func<IMessageBusScope> f) => messageBusScopeFactory = f);
            A.CallTo(() => _fakes.InstanceProvider.GetInstance<IMessageBusScope>())
                .ReturnsLazily(() => messageBusScopeFactory.Invoke());


            _sut = new BackendFxApplication(_fakes.CompositionRoot, _fakes.MessageBus, A.Fake<IExceptionLogger>());
        }

        [Fact]
        public void CanWaitForBoot()
        {
            var bootTime = 200;
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
            using (var scope = _sut.CompositionRoot.BeginScope())
            {
                var domainEventAggregator = scope.InstanceProvider.GetInstance<IDomainEventAggregator>();
                Assert.NotNull(domainEventAggregator);
            }

            A.CallTo(() => _fakes.InstanceProvider.GetInstance<IDomainEventAggregator>()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ProvidesExceptionLoggingInvoker()
        {
            Assert.IsType<ExceptionLoggingInvoker>(_sut.Invoker);
        }

        [Fact]
        public void IntegratesWithMessageBus()
        {
            A.CallTo(() => _fakes.MessageBus.ProvideInvoker(A<IBackendFxApplicationInvoker>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ProvidesMessageBusScope()
        {
            using (var scope = _sut.CompositionRoot.BeginScope())
            {
                var messageBusScope = scope.InstanceProvider.GetInstance<IMessageBusScope>();
                Assert.NotNull(messageBusScope);
            }

            A.CallTo(() => _fakes.InstanceProvider.GetInstance<IMessageBusScope>()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RegistersInfrastructureModule()
        {
            A.CallTo(
                    () => _fakes.InfrastructureModule
                        .RegisterScoped<ICurrentTHolder<Correlation>, CurrentCorrelationHolder>())
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.InfrastructureModule.RegisterScoped(A<Func<IDomainEventAggregator>>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(
                    () => _fakes.InfrastructureModule
                        .RegisterScoped<ICurrentTHolder<IIdentity>, CurrentIdentityHolder>())
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.InfrastructureModule.RegisterScoped(A<Func<IMessageBusScope>>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(
                    () => _fakes.InfrastructureModule
                        .RegisterScoped<ICurrentTHolder<TenantId>, CurrentTenantIdHolder>())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void VerifiesCompositionRootOnBoot()
        {
            _sut.BootAsync();
            A.CallTo(() => _fakes.CompositionRoot.Verify()).MustHaveHappenedOnceExactly();
        }
    }
}
