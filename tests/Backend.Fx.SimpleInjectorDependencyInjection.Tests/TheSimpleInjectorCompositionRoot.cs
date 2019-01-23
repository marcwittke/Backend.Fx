using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.IdGeneration;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain;
using FakeItEasy;
using SimpleInjector;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheSimpleInjectorCompositionRoot : IDisposable
    {
        private readonly SimpleInjectorCompositionRoot _sut;
        
        private class ADomainModule  : DomainModule 
        {
            public ADomainModule(params Assembly[] domainAssemblies) : base(domainAssemblies)
            { }

            protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
            {
                base.Register(container, scopedLifestyle);
                container.Register<IClock, FrozenClock>();
                container.RegisterInstance(A.Fake<IEventBus>());
            }
        }

        public TheSimpleInjectorCompositionRoot()
        {

            var tenantManager = A.Fake<ITenantManager>();
            TenantId[] tenantIds = { new TenantId(999) };
            A.CallTo(() => tenantManager.GetTenantIds()).Returns(tenantIds);


            _sut = new SimpleInjectorCompositionRoot();
            var domainAssembly = typeof(AnAggregate).GetTypeInfo().Assembly;
            _sut.RegisterModules(
                new ADomainModule(domainAssembly),
                new APersistenceModule(domainAssembly));
            
            _sut.Verify();

        }

        [Fact]
        public void ProvidesAutoRegisteredDomainServices()
        {
            using (var scope = _sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                var testDomainService = scope.GetInstance<ITestDomainService>();
                Assert.IsType<ADomainService>(testDomainService);
            }
        }

        [Fact]
        public void ProvidesAutoRegisteredDomainServicesThatImplementTwoInterfaces()
        {
            using (var scope = _sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                var testDomainService = scope.GetInstance<ITestDomainService>();
                Assert.IsType<ADomainService>(testDomainService);

                var anotherTestDomainService = scope.GetInstance<IAnotherTestDomainService>();
                Assert.IsType<ADomainService>(anotherTestDomainService);

                Assert.True(Equals(testDomainService, anotherTestDomainService));
            }
        }

        [Fact]
        public void ProvidesAutoRegisteredApplicationServices()
        {
            using (var scope = _sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                Assert.IsType<AnApplicationService>(scope.GetInstance<ITestApplicationService>());
            }
        }

        [Fact]
        public void MaintainsTenantIdWhenBeginningScopes()
        {
            Enumerable.Range(1, 100).AsParallel().ForAll(i =>
            {
                using (var scope = _sut.BeginScope(new SystemIdentity(), new TenantId(i)))
                {
                    var insideScopeTenantId = scope.GetInstance<ICurrentTHolder<TenantId>>().Current;
                    Assert.True(insideScopeTenantId.HasValue);
                    Assert.Equal(i, insideScopeTenantId.Value);
                }
            });
        }

        [Fact]
        public void MaintainsIdentityWhenBeginningScopes()
        {
            Enumerable.Range(1, 100).AsParallel().ForAll(i =>
            {
                using (var scope = _sut.BeginScope(new GenericIdentity(i.ToString()), new TenantId(100)))
                {
                    var insideScopeIdentity = scope.GetInstance<ICurrentTHolder<IIdentity>>().Current;
                    Assert.Equal(i.ToString(), insideScopeIdentity.Name);
                }
            });
        }

        [Fact]
        public void ProvidesScopedInstancesWhenScopeHasBeenStarted()
        {
            IClock scope1Clock;
            IClock scope2Clock;

            using (var scope = _sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                scope1Clock = scope.GetInstance<IClock>();
                Assert.NotNull(scope1Clock);
            }

            using (var scope = _sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                scope2Clock = scope.GetInstance<IClock>();
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

            ManualResetEvent waiter = new ManualResetEvent(false);

            // resolving a singleton service and a scoped service in a massive parallel scenario
            for (int index = 0; index < parallelScopeCount; index++)
            {
                var indexClosure = index;
                tasks[index] = Task.Factory.StartNew(() =>
                {
                    // using the reset event to enforce a maximum grade of parallelism
                    waiter.WaitOne();
                    using (var scope = _sut.BeginScope(new SystemIdentity(), new TenantId(null)))
                    {
                        scopedInstances[indexClosure] = scope.GetInstance<IClock>();
                        singletonInstances[indexClosure] = scope.GetInstance<IEntityIdGenerator>();
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

            using (var scope = _sut.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                var sutClock = _sut.GetInstance<IClock>();
                var scopeClock = scope.GetInstance<IClock>();
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
            using (_sut.BeginScope(new SystemIdentity(), new TenantId(1)))
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
