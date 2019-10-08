namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using FakeItEasy;
    using Fx.Environment.MultiTenancy;
    using Fx.Logging;
    using Fx.Patterns.DependencyInjection;
    using Fx.Patterns.EventAggregation.Integration;
    using JetBrains.Annotations;
    using Xunit;

    public sealed class TheInMemoryEventBus : TheEventBus
    {
        protected override IEventBus Create(IBackendFxApplication application)
        {
            return new InMemoryEventBus(application);
        }

        [Fact]
        public void HandlesEventsAsynchronously()
        {
            Sut.Subscribe<LongRunningEventHandler, TestIntegrationEvent>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var integrationEvent = new TestIntegrationEvent(1,"a");
            Sut.Publish(integrationEvent);
            Assert.True(sw.ElapsedMilliseconds < 900);
            integrationEvent.Processed.Wait(Debugger.IsAttached ? int.MaxValue : 5000);
            Assert.True(sw.ElapsedMilliseconds > 1000);
        }
    }

    [UsedImplicitly]
    public sealed class TheSerializingEventBus : TheEventBus
    {
        protected override IEventBus Create(IBackendFxApplication scopeManager)
        {
            return new SerializingEventBus(scopeManager);
        }
    }

    public abstract class TheEventBus
    {
        private readonly AutoResetEvent _exceptionHandled = new AutoResetEvent(false);
        private readonly AFakeApplication _app;
        protected IEventBus Sut { get; }

        protected TheEventBus()
        {
            _app = new AFakeApplication(_exceptionHandled);
            // ReSharper disable once VirtualMemberCallInConstructor
            Sut = Create(_app);
        }

        protected abstract IEventBus Create(IBackendFxApplication application);

        [Fact]
        public void CallsDynamicEventHandler()
        {
            Sut.Subscribe<DynamicEventHandler>(typeof(TestIntegrationEvent).FullName);
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            Sut.Publish(integrationEvent);
            integrationEvent.Processed.Wait(Debugger.IsAttached ? int.MaxValue : 5000);

            A.CallTo(() => _app.TypedHandler.Handle(A<TestIntegrationEvent>._)).MustNotHaveHappened();
            A.CallTo(() => _app.DynamicHandler.Handle(A<object>._)).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void CallsMixedEventHandlers()
        {
            Sut.Subscribe<DynamicEventHandler>(typeof(TestIntegrationEvent).FullName);
            Sut.Subscribe<TypedEventHandler, TestIntegrationEvent>();
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            Sut.Publish(integrationEvent);
            integrationEvent.Processed.Wait(Debugger.IsAttached ? int.MaxValue : 5000);

            A.CallTo(() => _app.TypedHandler.Handle(A<TestIntegrationEvent>
                    .That
                    .Matches(evt => evt.IntParam == 34 && evt.StringParam == "gaga")))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _app.DynamicHandler.Handle(A<object>._)).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void CallsTypedEventHandler()
        {
            Sut.Subscribe<TypedEventHandler, TestIntegrationEvent>();
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            Sut.Publish(integrationEvent);
            integrationEvent.Processed.Wait(Debugger.IsAttached ? int.MaxValue : 5000);
            A.CallTo(() => _app.TypedHandler.Handle(A<TestIntegrationEvent>
                                                   .That
                                                   .Matches(evt => evt.IntParam == 34 && evt.StringParam == "gaga")))
             .MustHaveHappenedOnceExactly();

            A.CallTo(() => _app.DynamicHandler.Handle(A<object>._)).MustNotHaveHappened();
        }
        
        [Fact]
        public void HandlesExceptionFromDynamicEventHandler()
        {
            Sut.Subscribe<ThrowingDynamicEventHandler>(typeof(TestIntegrationEvent).FullName);
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            Sut.Publish(integrationEvent);
            _exceptionHandled.WaitOne(Debugger.IsAttached ? int.MaxValue : 5000);

            A.CallTo(() => _app.ExceptionLogger.LogException(A<NotSupportedException>
                    .That
                    .Matches(ex => ex.Message == ThrowingDynamicEventHandler.ExceptionMessage)))
                .MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void HandlesExceptionFromTypedEventHandler()
        {
            Sut.Subscribe<ThrowingTypedEventHandler, TestIntegrationEvent>();
            var integrationEvent = new TestIntegrationEvent(34, "gaga");
            Sut.Publish(integrationEvent);
            _exceptionHandled.WaitOne(Debugger.IsAttached ? int.MaxValue : 5000);

            
            A.CallTo(() => _app.ExceptionLogger.LogException(A<NotSupportedException>
                                                            .That
                                                            .Matches(ex => ex.Message == ThrowingTypedEventHandler.ExceptionMessage)))
             .MustHaveHappenedOnceExactly();
        }

        private class AFakeApplication : BackendFxApplication
        {
            public IIntegrationEventHandler<TestIntegrationEvent> TypedHandler { get; } = A.Fake<IIntegrationEventHandler<TestIntegrationEvent>>();
            public IIntegrationEventHandler DynamicHandler { get; } = A.Fake<IIntegrationEventHandler>();
            
            public AFakeApplication(AutoResetEvent exceptionHandled) : this (A.Fake<ICompositionRoot>(), exceptionHandled)
            {}

            private AFakeApplication(ICompositionRoot compositionRoot, AutoResetEvent exceptionHandled) 
                : base(compositionRoot, A.Fake<ITenantIdService>(), A.Fake<IExceptionLogger>())
            {
                A.CallTo(() => ExceptionLogger.LogException(A<Exception>._)).Invokes(()=>exceptionHandled.Set());
                
                A.CallTo(() => CompositionRoot.BeginScope())
                    .Returns(A.Fake<IDisposable>());

                A.CallTo(() => CompositionRoot.GetInstance(A<Type>.That.IsEqualTo(typeof(TypedEventHandler))))
                 .Returns(new TypedEventHandler(TypedHandler));

                A.CallTo(() => CompositionRoot.GetInstance(A<Type>.That.IsEqualTo(typeof(LongRunningEventHandler))))
                 .Returns(new LongRunningEventHandler());

                A.CallTo(() => CompositionRoot.GetInstance(A<Type>.That.IsEqualTo(typeof(ThrowingTypedEventHandler))))
                 .Returns(new ThrowingTypedEventHandler());

                A.CallTo(() => CompositionRoot.GetInstance(A<Type>.That.IsEqualTo(typeof(DynamicEventHandler))))
                 .Returns(new DynamicEventHandler(DynamicHandler));

                A.CallTo(() => CompositionRoot.GetInstance(A<Type>.That.IsEqualTo(typeof(ThrowingDynamicEventHandler))))
                 .Returns(new ThrowingDynamicEventHandler());
            }
        }
    }
}
