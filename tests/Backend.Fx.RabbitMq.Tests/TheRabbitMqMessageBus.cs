using System;
using System.Diagnostics;
using System.Threading;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using RabbitMQ.Client;
using Xunit;

namespace Backend.Fx.RabbitMq.Tests
{
    public class TheRabbitMqMessageBus
    {
        private readonly ManualResetEvent _received = new ManualResetEvent(false);
        private readonly BackendFxApplicationInvoker _senderInvoker;
        private readonly BackendFxApplicationInvoker _receiverInvoker;

        public TheRabbitMqMessageBus()
        {
            var fakeSenderApplication = A.Fake<IBackendFxApplication>();
            _senderInvoker = new BackendFxApplicationInvoker(fakeSenderApplication.CompositionRoot);


            var fakeReceiverApplication = A.Fake<IBackendFxApplication>();
            _receiverInvoker = new BackendFxApplicationInvoker(fakeReceiverApplication.CompositionRoot);
            var fakeScope = A.Fake<IInjectionScope>();
            var fakeInstanceProvider = A.Fake<IInstanceProvider>();
            A.CallTo(() => fakeReceiverApplication.CompositionRoot.BeginScope()).Returns(fakeScope);
            A.CallTo(() => fakeScope.InstanceProvider).Returns(fakeInstanceProvider);
            A.CallTo(() => fakeInstanceProvider.GetInstance(A<Type>.That.IsEqualTo(typeof(TestIntegrationEventHandler))))
             .Returns(new TestIntegrationEventHandler(_received));
        }

        //[Fact]
        public void CanBeUsedWithBackendFxApplication()
        {
            IMessageBus sender = new RabbitMqMessageBus(new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "anicors",
                Password = "R4bb!tMQ"
            }, 5, "unittest", "testSender");
            sender.IntegrateApplication(_senderInvoker);
            sender.Connect();

            var receiver = new RabbitMqMessageBus(new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "anicors",
                Password = "R4bb!tMQ"
            }, 5, "unittest", "testReceiver");

            receiver.IntegrateApplication(_receiverInvoker);
            receiver.Connect();
            receiver.Subscribe<TestIntegrationEventHandler, TestIntegrationEvent>();

            sender.Publish(new TestIntegrationEvent(1));
            Assert.True(_received.WaitOne(Debugger.IsAttached ? int.MaxValue : 5000));
        }

        public class TestIntegrationEventHandler : IIntegrationMessageHandler<TestIntegrationEvent>
        {
            private readonly ManualResetEvent _received;

            public TestIntegrationEventHandler(ManualResetEvent received)
            {
                _received = received;
            }

            public void Handle(TestIntegrationEvent eventData)
            {
                _received.Set();
            }
        }

        public class TestIntegrationEvent : IntegrationEvent
        {
            public TestIntegrationEvent(int sequencenumber) : base(999)
            {
                Sequencenumber = sequencenumber;
            }

            public int Sequencenumber { get; }
        }
    }
}