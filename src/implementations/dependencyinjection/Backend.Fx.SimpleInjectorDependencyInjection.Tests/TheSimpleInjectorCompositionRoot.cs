using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain;
using FakeItEasy;
using SimpleInjector;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheSimpleInjectorCompositionRoot : IDisposable
    {
        private readonly SimpleInjectorCompositionRoot _sut;

        public TheSimpleInjectorCompositionRoot()
        {
            var domainAssembly = typeof(AnAggregate).GetTypeInfo().Assembly;
            _sut = new SimpleInjectorCompositionRoot(A.Fake<IMessageBus>(), domainAssembly);
            _sut.Container.RegisterPackages(new[] { domainAssembly });
            _sut.Verify();
        }

        public void Dispose()
        {
            _sut.Dispose();
        }

        [Fact]
        public void ProvidesAutoRegisteredDomainServices()
        {
            using (_sut.BeginScope())
            {
                var testDomainService = _sut.GetInstance<ITestDomainService>();
                Assert.IsType<ADomainService>(testDomainService);
            }
        }

        [Fact]
        public void ProvidesAutoRegisteredDomainServicesThatImplementTwoInterfaces()
        {
            using (_sut.BeginScope())
            {
                var testDomainService = _sut.GetInstance<ITestDomainService>();
                Assert.IsType<ADomainService>(testDomainService);

                var anotherTestDomainService = _sut.GetInstance<IAnotherTestDomainService>();
                Assert.IsType<ADomainService>(anotherTestDomainService);

                Assert.True(Equals(testDomainService, anotherTestDomainService));
            }
        }

        [Fact]
        public void ProvidesAutoRegisteredApplicationServices()
        {
            using (_sut.BeginScope())
            {
                Assert.IsType<AnApplicationService>(_sut.GetInstance<ITestApplicationService>());
            }
        }

        [Fact]
        public void ProvidesScopedInstancesWhenScopeHasBeenStarted()
        {
            ITestDomainService scope1Instance;
            ITestDomainService scope2Instance;

            using (var scope = _sut.BeginScope())
            {
                scope1Instance = scope.InstanceProvider.GetInstance<ITestDomainService>();
                Assert.NotNull(scope1Instance);
            }

            using (var scope = _sut.BeginScope())
            {
                scope2Instance = scope.InstanceProvider.GetInstance<ITestDomainService>();
                Assert.NotNull(scope2Instance);
            }

            Assert.NotEqual(scope1Instance, scope2Instance);
        }

        [Fact]
        public void ProvidesSingletonAndScopedInstancesAccordingly()
        {
            const int parallelScopeCount = 1000;
            var scopedInstances = new object[parallelScopeCount];
            var singletonInstances = new object[parallelScopeCount];
            var tasks = new Task[parallelScopeCount];

            var waiter = new ManualResetEvent(false);

            // resolving a singleton service and a scoped service in a massive parallel scenario
            for (var index = 0; index < parallelScopeCount; index++)
            {
                int indexClosure = index;
                tasks[index] = Task.Factory.StartNew(
                    () =>
                    {
                        // using the reset event to enforce a maximum grade of parallelism
                        waiter.WaitOne();
                        using (var scope = _sut.BeginScope())
                        {
                            scopedInstances[indexClosure] = scope.InstanceProvider.GetInstance<ITestDomainService>();
                            singletonInstances[indexClosure] = scope.InstanceProvider.GetInstance<ISingletonService>();
                        }
                    });
            }

            // let the show begin...
            waiter.Set();
            Task.WaitAll(tasks);

            // asserting for equality: singleton instances must be equal, scoped instances must be unique
            for (var index = 0; index < parallelScopeCount; index++)
            {
                Assert.NotNull(scopedInstances[index]);
                Assert.NotNull(singletonInstances[index]);

                for (var indexComp = 0; indexComp < parallelScopeCount; indexComp++)
                {
                    if (index != indexComp)
                    {
                        Assert.NotEqual(scopedInstances[index], scopedInstances[indexComp]);
                    }

                    Assert.Equal(singletonInstances[index], singletonInstances[indexComp]);
                }
            }
        }

        [Fact]
        public void ThrowsWhenScopedInstanceIsRequestedOutsideScope()
        {
            Assert.Throws<ActivationException>(() => _sut.GetInstance<ITestDomainService>());
            Assert.Throws<ActivationException>(() => _sut.GetInstance(typeof(ITestDomainService)));

            using (var scope = _sut.BeginScope())
            {
                var sutInstance = scope.InstanceProvider.GetInstance<ITestDomainService>();
                var scopeInstance = scope.InstanceProvider.GetInstance<ITestDomainService>();
                Assert.NotNull(sutInstance);
                Assert.NotNull(scopeInstance);
                Assert.Equal(sutInstance, scopeInstance);
            }

            Assert.Throws<ActivationException>(() => _sut.GetInstance<ITestDomainService>());
            Assert.Throws<ActivationException>(() => _sut.GetInstance(typeof(ITestDomainService)));
        }

        [Fact]
        public void CanProvideEventHandlers()
        {
            var aDomainEvent = new ADomainEvent();

            using (var scope = _sut.BeginScope())
            {
                var domainEventAggregator = scope.InstanceProvider.GetInstance<IDomainEventAggregator>();
                domainEventAggregator.PublishDomainEvent(aDomainEvent);
                domainEventAggregator.RaiseEvents();
            }

            Assert.Contains(typeof(ADomainEventHandler1), aDomainEvent.HandledBy);
            Assert.Contains(typeof(ADomainEventHandler2), aDomainEvent.HandledBy);
            Assert.Contains(typeof(ADomainEventHandler3), aDomainEvent.HandledBy);
        }
    }
}
