using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain;
using SimpleInjector;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheSimpleInjectorCompositionRoot : IDisposable
    {
        private readonly SimpleInjectorCompositionRoot _sut;
        
        public TheSimpleInjectorCompositionRoot()
        {
            _sut = new SimpleInjectorCompositionRoot();
            Assembly domainAssembly = typeof(AnAggregate).GetTypeInfo().Assembly;
            _sut.RegisterModules(new ADomainModule(domainAssembly));
            _sut.Verify();
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

            using (_sut.BeginScope())
            {
                scope1Instance = _sut.GetInstance<ITestDomainService>();
                Assert.NotNull(scope1Instance);
            }

            using (_sut.BeginScope())
            {
                scope2Instance = _sut.GetInstance<ITestDomainService>();
                Assert.NotNull(scope2Instance);
            }

            Assert.NotEqual(scope1Instance, scope2Instance);
        }

        [Fact]
        public void ProvidesSingletonAndScopedInstancesAccordingly()
        {
            
            const int parallelScopeCount = 1000;
            object[] scopedInstances = new object[parallelScopeCount];
            object[] singletonInstances = new object[parallelScopeCount];
            Task[] tasks = new Task[parallelScopeCount];

            var waiter = new ManualResetEvent(false);

            // resolving a singleton service and a scoped service in a massive parallel scenario
            for (int index = 0; index < parallelScopeCount; index++)
            {
                var indexClosure = index;
                tasks[index] = Task.Factory.StartNew(() =>
                {
                    // using the reset event to enforce a maximum grade of parallelism
                    waiter.WaitOne();
                    using (_sut.BeginScope())
                    {
                        scopedInstances[indexClosure] = _sut.GetInstance<ITestDomainService>();
                        singletonInstances[indexClosure] = _sut.GetInstance<ISingletonService>();
                    }
                });
            }

            // let the show begin...
            waiter.Set();
            Task.WaitAll(tasks);

            // asserting for equality: singleton instances must be equal, scoped instances must be unique
            for (int index = 0; index < parallelScopeCount; index++)
            {
                Assert.NotNull(scopedInstances[index]);
                Assert.NotNull(singletonInstances[index]);

                for (int indexComp = 0; indexComp < parallelScopeCount; indexComp++)
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
            Assert.Null(_sut.GetCurrentScope());

            using (_sut.BeginScope())
            {
                var sutInstance = _sut.GetInstance<ITestDomainService>();
                var scopeInstance = _sut.GetInstance<ITestDomainService>();
                Assert.NotNull(sutInstance);
                Assert.NotNull(scopeInstance);
                Assert.Equal(sutInstance, scopeInstance);
            }

            Assert.Null(_sut.GetCurrentScope());
            Assert.Throws<ActivationException>(() => _sut.GetInstance<ITestDomainService>());
            Assert.Throws<ActivationException>(() => _sut.GetInstance(typeof(ITestDomainService)));
        }

        [Fact]
        public void CanProvideEventHandlers()
        {
            using (_sut.BeginScope())
            {
                var handlers = _sut.GetAllEventHandlers<ADomainEvent>().ToArray();
                
                // these three handlers should have been auto registered during boot by scanning the assembly
                Assert.True(handlers.OfType<ADomainEventHandler1>().Any());
                Assert.True(handlers.OfType<ADomainEventHandler2>().Any());
                Assert.True(handlers.OfType<ADomainEventHandler3>().Any());
            }
        }
        
        public void Dispose()
        {
            _sut.Dispose();
        }
    }
}
