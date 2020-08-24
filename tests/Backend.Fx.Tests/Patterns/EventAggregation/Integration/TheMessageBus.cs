using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using JetBrains.Annotations;
using Xunit;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public sealed class TheInMemoryMessageBus : TheMessageBus
    {
        public TheInMemoryMessageBus()
        {
            Sut.ProvideInvoker(FakeApplication.Invoker);
        }
        
        [Fact]
        public async Task HandlesEventsAsynchronously()
        {
            Sut.Subscribe<LongRunningMessageHandler, TestIntegrationEvent>();
            var sw = new Stopwatch();
            sw.Start();
            await Sut.Publish(Invoker.IntegrationEvent);
            Assert.True(sw.ElapsedMilliseconds < 900);
            Assert.True(Invoker.IntegrationEvent.TypedProcessed.Wait(Debugger.IsAttached ? int.MaxValue : 10000));
            Assert.True(sw.ElapsedMilliseconds > 1000);
        }

        protected override IMessageBus Sut { get; } = new InMemoryMessageBus();
    }

    [UsedImplicitly]
    public sealed class TheSerializingMessageBus : TheMessageBus
    {
        public TheSerializingMessageBus()
        {
            Sut.ProvideInvoker(FakeApplication.Invoker);
        }

        protected override IMessageBus Sut { get; } = new SerializingMessageBus();
    }

    public abstract class TheMessageBus
    {
        protected IBackendFxApplication FakeApplication { get; } = A.Fake<IBackendFxApplication>();

        protected TheMessageBus()
        {
            A.CallTo(() => FakeApplication.Invoker).Returns(Invoker);
            A.CallTo(() => FakeApplication.WaitForBoot(A<int>._, A<CancellationToken>._)).Returns(true);
        }

        protected TestInvoker Invoker { get; } = new TestInvoker();
        protected abstract IMessageBus Sut { get; }


        public class TestInvoker : IBackendFxApplicationInvoker
        {
            public TestIntegrationEvent IntegrationEvent = new TestIntegrationEvent(34, "gaga");

            public TestInvoker()
            {
                A.CallTo(() => TypedHandler.Handle(A<TestIntegrationEvent>._)).Invokes((TestIntegrationEvent e) => IntegrationEvent.TypedProcessed.Set());
                A.CallTo(() => DynamicHandler.Handle(new object())).WithAnyArguments().Invokes((object e) => IntegrationEvent.DynamicProcessed.Set());

                A.CallTo(() => FakeInstanceProvider.GetInstance(A<Type>.That.IsEqualTo(typeof(TypedMessageHandler))))
                 .Returns(new TypedMessageHandler(TypedHandler));

                A.CallTo(() => FakeInstanceProvider.GetInstance(A<Type>.That.IsEqualTo(typeof(LongRunningMessageHandler))))
                 .Returns(new LongRunningMessageHandler(TypedHandler));

                A.CallTo(() => FakeInstanceProvider.GetInstance(A<Type>.That.IsEqualTo(typeof(ThrowingTypedMessageHandler))))
                 .Returns(new ThrowingTypedMessageHandler(TypedHandler));

                A.CallTo(() => FakeInstanceProvider.GetInstance(A<Type>.That.IsEqualTo(typeof(DynamicMessageHandler))))
                 .Returns(new DynamicMessageHandler(DynamicHandler));

                A.CallTo(() => FakeInstanceProvider.GetInstance(A<Type>.That.IsEqualTo(typeof(ThrowingDynamicMessageHandler))))
                 .Returns(new ThrowingDynamicMessageHandler(DynamicHandler));
            }

            public IIntegrationMessageHandler<TestIntegrationEvent> TypedHandler { get; } = A.Fake<IIntegrationMessageHandler<TestIntegrationEvent>>();
            public IIntegrationMessageHandler DynamicHandler { get; } = A.Fake<IIntegrationMessageHandler>();
            public IInstanceProvider FakeInstanceProvider { get; } = A.Fake<IInstanceProvider>();

            public void Invoke(Action<IInstanceProvider> action, IIdentity identity, TenantId tenantId, Guid? correlationId = null)
            {
                action(FakeInstanceProvider);
            }
        }

        [Fact]
        public async void CallsDynamicEventHandler()
        {
            Sut.Subscribe<DynamicMessageHandler>(Sut.MessageNameProvider.GetMessageName<TestIntegrationEvent>());
            await Sut.Publish(Invoker.IntegrationEvent);
            Assert.True(Invoker.IntegrationEvent.DynamicProcessed.Wait(Debugger.IsAttached ? int.MaxValue : 10000));

            A.CallTo(() => Invoker.TypedHandler.Handle(A<TestIntegrationEvent>._)).MustNotHaveHappened();
            A.CallTo(() => Invoker.DynamicHandler.Handle(A<object>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void CallsMixedEventHandlers()
        {
            Sut.Subscribe<DynamicMessageHandler>(Sut.MessageNameProvider.GetMessageName<TestIntegrationEvent>());
            Sut.Subscribe<TypedMessageHandler, TestIntegrationEvent>();

            await Sut.Publish(Invoker.IntegrationEvent);
            Assert.True(Invoker.IntegrationEvent.TypedProcessed.Wait(Debugger.IsAttached ? int.MaxValue : 10000));
            Assert.True(Invoker.IntegrationEvent.DynamicProcessed.Wait(Debugger.IsAttached ? int.MaxValue : 10000));

            A.CallTo(() => Invoker.TypedHandler.Handle(A<TestIntegrationEvent>
                                                       .That
                                                       .Matches(evt => evt.IntParam == 34 && evt.StringParam == "gaga")))
             .MustHaveHappenedOnceExactly();

            A.CallTo(() => Invoker.DynamicHandler.Handle(A<object>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CallsTypedEventHandler()
        {
            Sut.Subscribe<TypedMessageHandler, TestIntegrationEvent>();
            await Sut.Publish(Invoker.IntegrationEvent);
            Assert.True(Invoker.IntegrationEvent.TypedProcessed.Wait(Debugger.IsAttached ? int.MaxValue : 10000));
            A.CallTo(() => Invoker.TypedHandler.Handle(A<TestIntegrationEvent>
                                                       .That
                                                       .Matches(evt => evt.IntParam == 34 && evt.StringParam == "gaga")))
             .MustHaveHappenedOnceExactly();

            A.CallTo(() => Invoker.DynamicHandler.Handle(A<object>._)).MustNotHaveHappened();
        }
        
        [Fact]
        public async Task DoesNotCallUnsubscribedTypedEventHandler()
        {
            Sut.Subscribe<TypedMessageHandler, TestIntegrationEvent>();
            Sut.Unsubscribe<TypedMessageHandler, TestIntegrationEvent>();
            await Sut.Publish(Invoker.IntegrationEvent);
            A.CallTo(() => Invoker.TypedHandler.Handle(A<TestIntegrationEvent>
                                                       .That
                                                       .Matches(evt => evt.IntParam == 34 && evt.StringParam == "gaga")))
             .MustNotHaveHappened();
        }
        
        [Fact]
        public async void DoesNotCallUnsubscribedDynamicEventHandler()
        {
            Sut.Subscribe<DynamicMessageHandler>(Sut.MessageNameProvider.GetMessageName<TestIntegrationEvent>());
            Sut.Unsubscribe<DynamicMessageHandler>(Sut.MessageNameProvider.GetMessageName<TestIntegrationEvent>());
            await Sut.Publish(Invoker.IntegrationEvent);
            A.CallTo(() => Invoker.DynamicHandler.Handle(A<object>._)).MustNotHaveHappened();
        }
        
        [Fact]
        public async void DoesNotCallUnsubscribedDelegateEventHandler()
        {
            var handled = new ManualResetEvent(false);
            var handler = new DelegateIntegrationMessageHandler<TestIntegrationEvent>(ev => handled.Set());
            Sut.Subscribe(handler);
            Sut.Unsubscribe(handler);
            await Sut.Publish(Invoker.IntegrationEvent);
            Assert.False(handled.WaitOne(Debugger.IsAttached ? int.MaxValue : 1000));
        }

        [Fact]
        public void CannCallConnectButItDoesNothing()
        {
            Sut.Connect();
        }
        
        [Fact]
        public async void DelegateIntegrationMessageHandler()
        {
            var handled = new ManualResetEvent(false);
            var handler = new DelegateIntegrationMessageHandler<TestIntegrationEvent>(ev => handled.Set());
            Sut.Subscribe(handler);
            await Sut.Publish(Invoker.IntegrationEvent);
            Assert.True(handled.WaitOne(Debugger.IsAttached ? int.MaxValue : 10000));
        }
    }
}