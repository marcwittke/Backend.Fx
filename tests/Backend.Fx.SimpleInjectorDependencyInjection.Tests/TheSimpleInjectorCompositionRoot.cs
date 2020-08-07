using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.IdGeneration;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain;
using SimpleInjector;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheSimpleInjectorCompositionRoot : IDisposable
    {
        private readonly SimpleInjectorCompositionRoot _sut;
        
        private class ADomainModule  : SimpleInjectorDomainModule 
        {
            public ADomainModule(params Assembly[] domainAssemblies) : base(domainAssemblies)
            { }

            protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
            {
                base.Register(container, scopedLifestyle);
                container.Register<IClock, FrozenClock>();
            }
        }

        public TheSimpleInjectorCompositionRoot()
        {
            _sut = new SimpleInjectorCompositionRoot();
            Assembly domainAssembly = typeof(AnAggregate).GetTypeInfo().Assembly;
            _sut.RegisterModules(
                new ADomainModule(domainAssembly),
                new APersistenceModule(domainAssembly));
            
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
            IClock scope1Clock;
            IClock scope2Clock;

            using (_sut.BeginScope())
            {
                scope1Clock = _sut.GetInstance<IClock>();
                Assert.NotNull(scope1Clock);
            }

            using (_sut.BeginScope())
            {
                scope2Clock = _sut.GetInstance<IClock>();
                Assert.NotNull(scope2Clock);
            }

            Assert.NotEqual(scope1Clock, scope2Clock);
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
                        scopedInstances[indexClosure] = _sut.GetInstance<IClock>();
                        singletonInstances[indexClosure] = _sut.GetInstance<IEntityIdGenerator>();
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
            Assert.Throws<ActivationException>(() => _sut.GetInstance<IClock>());
            Assert.Throws<ActivationException>(() => _sut.GetInstance(typeof(IClock)));
            Assert.Null(_sut.GetCurrentScope());

            using (_sut.BeginScope())
            {
                var sutClock = _sut.GetInstance<IClock>();
                var scopeClock = _sut.GetInstance<IClock>();
                Assert.NotNull(sutClock);
                Assert.NotNull(scopeClock);
                Assert.Equal(sutClock, scopeClock);
            }

            Assert.Null(_sut.GetCurrentScope());
            Assert.Throws<ActivationException>(() => _sut.GetInstance<IClock>());
            Assert.Throws<ActivationException>(() => _sut.GetInstance(typeof(IClock)));
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
