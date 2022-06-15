using System;
using System.Diagnostics;
using System.Threading;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.RabbitMq.Tests
{
    public class TheRabbitMqMessageBus : TestWithLogging
    {
        private readonly ManualResetEvent _received = new ManualResetEvent(false);
        private readonly BackendFxApplicationInvoker _senderInvoker;
        private readonly BackendFxApplicationInvoker _receiverInvoker;

        public TheRabbitMqMessageBus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            var fakeSenderApplication = A.Fake<IBackendFxApplication>();
            _senderInvoker = new BackendFxApplicationInvoker(fakeSenderApplication.CompositionRoot);


            var fakeReceiverApplication = A.Fake<IBackendFxApplication>();
            _receiverInvoker = new BackendFxApplicationInvoker(fakeReceiverApplication.CompositionRoot);
            var fakeScope = A.Fake<IServiceScope>();
            var fakeServiceProvider = A.Fake<IServiceProvider>();
            A.CallTo(() => fakeReceiverApplication.CompositionRoot.BeginScope()).Returns(fakeScope);
            A.CallTo(() => fakeScope.ServiceProvider).Returns(fakeServiceProvider);
            A.CallTo(() => fakeServiceProvider.GetService(A<Type>.That.IsEqualTo(typeof(TestIntegrationEventHandler))))
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
            sender.ProvideInvoker(_senderInvoker);
            sender.Connect();

            var receiver = new RabbitMqMessageBus(new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "anicors",
                Password = "R4bb!tMQ"
            }, 5, "unittest", "testReceiver");

            receiver.ProvideInvoker(_receiverInvoker);
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
            public TestIntegrationEvent(int sequencenumber) : base()
            {
                Sequencenumber = sequencenumber;
            }

            public int Sequencenumber { get; }
        }
    }
}