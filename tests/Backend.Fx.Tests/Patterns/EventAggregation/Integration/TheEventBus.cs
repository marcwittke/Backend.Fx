using JetBrains.Annotations;
using Xunit;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System;
    using System.Diagnostics;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Fx.Environment.MultiTenancy;
    using Fx.Logging;
    using Fx.Patterns.DependencyInjection;
    using Fx.Patterns.EventAggregation.Integration;

    public sealed class TheInMemoryEventBus : TheEventBus
    {

        protected override IEventBus Create(IScopeManager scopeManager, IExceptionLogger exceptionLogger)
        {
            return new InMemoryEventBus(scopeManager, exceptionLogger);
        }

        [Fact]
        public async Task HandlesEventsAsynchronously()
        {
            Sut.Subscribe<LongRunningEventHandler, TestIntegrationEvent>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var integrationEvent = new TestIntegrationEvent(1,"a");
            await Sut.Publish(integrationEvent);
            Assert.True(sw.ElapsedMilliseconds < 100);
            integrationEvent.Processed.Wait(1500);
            Assert.True(sw.ElapsedMilliseconds > 1000);
        }
    }

    [UsedImplicitly]
    public sealed class TheSerializingEventBus : TheEventBus
    {
        protected override IEventBus Create(IScopeManager scopeManager, IExceptionLogger exceptionLogger)
        {
            return new SerializingEventBus(scopeManager, exceptionLogger);
        }
    }

    public abstract class TheEventBus
    {
        private readonly EventBusFakeInjection _inj = new EventBusFakeInjection();
        public IEventBus Sut { get; }

        protected TheEventBus()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Sut = Create(_inj.ScopeManager, _inj.ExceptionLogger);
        }

        protected abstract IEventBus Create(IScopeManager scopeManager, IExceptionLogger exceptionLogger);

        [Fact]
        public async Task CallsTypedEventHandler()
        {
            Sut.Subscribe<TypedEventHandler, TestIntegrationEvent>();
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            await Sut.Publish(integrationEvent);
            integrationEvent.Processed.Wait(1500);
            A.CallTo(() => _inj.TypedHandler.Handle(A<TestIntegrationEvent>
                                                   .That
                                                   .Matches(evt => evt.IntParam == 34 && evt.StringParam == "gaga")))
             .MustHaveHappenedOnceExactly();

            A.CallTo(() => _inj.DynamicHandler.Handle(A<object>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void HandlesExceptionFromTypedEventHandler()
        {
            Sut.Subscribe<ThrowingTypedEventHandler, TestIntegrationEvent>();
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            await Sut.Publish(integrationEvent);
            integrationEvent.Processed.Wait(1500);

            A.CallTo(() => _inj.ExceptionLogger.LogException(A<InvalidOperationException>
                                                            .That
                                                            .Matches(ex => ex.Message == ThrowingTypedEventHandler.ExceptionMessage)))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void CallsDynamicEventHandler()
        {
            Sut.Subscribe<DynamicEventHandler>(typeof(TestIntegrationEvent).FullName);
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            await Sut.Publish(integrationEvent);
            integrationEvent.Processed.Wait(1500);

            A.CallTo(() => _inj.TypedHandler.Handle(A<TestIntegrationEvent>._)).MustNotHaveHappened();
            A.CallTo(() => _inj.DynamicHandler.Handle(A<object>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void HandlesExceptionFromDynamicEventHandler()
        {
            Sut.Subscribe<ThrowingDynamicEventHandler>(typeof(TestIntegrationEvent).FullName);
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            await Sut.Publish(integrationEvent);
            integrationEvent.Processed.Wait(1500);

            A.CallTo(() => _inj.ExceptionLogger.LogException(A<InvalidOperationException>
                                                        .That
                                                        .Matches(ex => ex.Message == ThrowingDynamicEventHandler.ExceptionMessage)))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void CallsMixedEventHandlers()
        {
            Sut.Subscribe<DynamicEventHandler>(typeof(TestIntegrationEvent).FullName);
            Sut.Subscribe<TypedEventHandler, TestIntegrationEvent>();
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            await Sut.Publish(integrationEvent);
            integrationEvent.Processed.Wait(1500);

            A.CallTo(() => _inj.TypedHandler.Handle(A<TestIntegrationEvent>
                                                   .That
                                                   .Matches(evt => evt.IntParam == 34 && evt.StringParam == "gaga")))
             .MustHaveHappenedOnceExactly();

            A.CallTo(() => _inj.DynamicHandler.Handle(A<object>._)).MustHaveHappenedOnceExactly();
        }

        private class EventBusFakeInjection
        {
            public IScopeManager ScopeManager { get; } = A.Fake<IScopeManager>();
            public IScope Scope { get; } = A.Fake<IScope>();
            public IIntegrationEventHandler<TestIntegrationEvent> TypedHandler { get; } = A.Fake<IIntegrationEventHandler<TestIntegrationEvent>>();
            public IIntegrationEventHandler DynamicHandler { get; } = A.Fake<IIntegrationEventHandler>();
            public IExceptionLogger ExceptionLogger { get; } = A.Fake<IExceptionLogger>();

            public EventBusFakeInjection()
            {
                A.CallTo(() => ScopeManager.BeginScope(A<IIdentity>._, A<TenantId>._))
                 .Returns(Scope);

                A.CallTo(() => Scope.GetInstance(A<Type>.That.IsEqualTo(typeof(TypedEventHandler))))
                 .Returns(new TypedEventHandler(TypedHandler));

                A.CallTo(() => Scope.GetInstance(A<Type>.That.IsEqualTo(typeof(LongRunningEventHandler))))
                 .Returns(new LongRunningEventHandler());

                A.CallTo(() => Scope.GetInstance(A<Type>.That.IsEqualTo(typeof(ThrowingTypedEventHandler))))
                 .Returns(new ThrowingTypedEventHandler());

                A.CallTo(() => Scope.GetInstance(A<Type>.That.IsEqualTo(typeof(DynamicEventHandler))))
                 .Returns(new DynamicEventHandler(DynamicHandler));

                A.CallTo(() => Scope.GetInstance(A<Type>.That.IsEqualTo(typeof(ThrowingDynamicEventHandler))))
                 .Returns(new ThrowingDynamicEventHandler());
            }
        }
    }
}
