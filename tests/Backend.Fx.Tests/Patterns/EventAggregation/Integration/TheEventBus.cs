namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Fx.Environment.MultiTenancy;
    using Fx.Logging;
    using Fx.Patterns.DependencyInjection;
    using Fx.Patterns.EventAggregation.Integration;
    using Xunit;

    public sealed class TheInMemoryEventBus : TheEventBus
    {
        protected override IEventBus Create(IScopeManager scopeManager, IExceptionLogger exceptionLogger)
        {
            return new InMemoryEventBus(scopeManager, exceptionLogger);
        }
    }

    public sealed class TheSerializingEventBus : TheEventBus
    {
        protected override IEventBus Create(IScopeManager scopeManager, IExceptionLogger exceptionLogger)
        {
            return new SerializingEventBus(scopeManager, exceptionLogger);
        }
    }

    public abstract class TheEventBus
    {
        private readonly EventBusFakeInjection inj = new EventBusFakeInjection();
        private readonly IEventBus sut;

        protected TheEventBus()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            sut = Create(inj.ScopeManager, inj.ExceptionLogger);
        }

        protected abstract IEventBus Create(IScopeManager scopeManager, IExceptionLogger exceptionLogger);

        [Fact]
        public async Task CallsTypedEventHandler()
        {
            sut.Subscribe<TypedEventHandler, TestIntegrationEvent>();
            await sut.Publish(new TestIntegrationEvent(34, "gaga"));

            A.CallTo(() => inj.TypedHandler.Handle(A<TestIntegrationEvent>
                                                   .That
                                                   .Matches(evt => evt.IntParam == 34 && evt.StringParam == "gaga")))
             .MustHaveHappenedOnceExactly();

            A.CallTo(() => inj.DynamicHandler.Handle(A<object>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void HandlesExceptionFromTypedEventHandler()
        {
            sut.Subscribe<ThrowingTypedEventHandler, TestIntegrationEvent>();
            await sut.Publish(new TestIntegrationEvent(34, "gaga"));

            A.CallTo(() => inj.ExceptionLogger.LogException(A<InvalidOperationException>
                                                            .That
                                                            .Matches(ex => ex.Message == ThrowingTypedEventHandler.ExceptionMessage)))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void CallsDynamicEventHandler()
        {
            sut.Subscribe<DynamicEventHandler>(typeof(TestIntegrationEvent).FullName);
            await sut.Publish(new TestIntegrationEvent(34, "gaga"));
            A.CallTo(() => inj.TypedHandler.Handle(A<TestIntegrationEvent>._)).MustNotHaveHappened();
            A.CallTo(() => inj.DynamicHandler.Handle(A<object>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void HandlesExceptionFromDynamicEventHandler()
        {
            sut.Subscribe<ThrowingDynamicEventHandler>(typeof(TestIntegrationEvent).FullName);
            await sut.Publish(new TestIntegrationEvent(34, "gaga"));

            A.CallTo(() => inj.ExceptionLogger.LogException(A<InvalidOperationException>
                                                        .That
                                                        .Matches(ex => ex.Message == ThrowingDynamicEventHandler.ExceptionMessage)))
             .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void CallsMixedEventHandlers()
        {
            sut.Subscribe<DynamicEventHandler>(typeof(TestIntegrationEvent).FullName);
            sut.Subscribe<TypedEventHandler, TestIntegrationEvent>();
            await sut.Publish(new TestIntegrationEvent(34, "gaga"));
            A.CallTo(() => inj.TypedHandler.Handle(A<TestIntegrationEvent>
                                                   .That
                                                   .Matches(evt => evt.IntParam == 34 && evt.StringParam == "gaga")))
             .MustHaveHappenedOnceExactly();

            A.CallTo(() => inj.DynamicHandler.Handle(A<object>._)).MustHaveHappenedOnceExactly();
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
