using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Extensions.MessageBus;
using Backend.Fx.Extensions.MessageBus.InProc;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class TheInMemoryMessageBusChannel : TestWithLogging
    {
        [Fact]
        public async Task HandlesEventsAsynchronously()
        {
            var channel = new InProcMessageBusChannel();
            var messageBus = new InProcMessageBus(channel);
            messageBus.Connect();
            messageBus.ProvideInvoker(new TheMessageBus.TestInvoker());

            var handled = new ManualResetEvent(false);
            messageBus.Subscribe(new BlockingEventHandler(handled));

            await messageBus.PublishAsync(new TestIntegrationEvent(0, string.Empty));

            var finishHandleTask = channel.FinishHandlingAllMessagesAsync();
            Assert.Contains(finishHandleTask.Status, new[] {TaskStatus.WaitingForActivation, TaskStatus.Running});
            handled.Set();

            await finishHandleTask;
        }
        
        [Fact]
        public async Task InvokesAllApplicationHandlers()
        {
            var channel = new InProcMessageBusChannel();
            
            var messageBus = new InProcMessageBus(channel);
            var eventHandled = false;
            messageBus.Connect();
            messageBus.ProvideInvoker(new TheMessageBus.TestInvoker());
            messageBus.Subscribe(new DelegateIntegrationEventHandler<TestIntegrationEvent>(_ => eventHandled = true));
            
            var anotherMessageBus = new InProcMessageBus(channel);
            var anotherEventHandled = false;
            anotherMessageBus.Connect();
            anotherMessageBus.ProvideInvoker(new TheMessageBus.TestInvoker());
            messageBus.Subscribe(new DelegateIntegrationEventHandler<TestIntegrationEvent>(_ => anotherEventHandled = true));

            await messageBus.PublishAsync(new TestIntegrationEvent(0, string.Empty));
            await channel.FinishHandlingAllMessagesAsync();

            Assert.True(eventHandled);
            Assert.True(anotherEventHandled);

            eventHandled = false;
            anotherEventHandled = false;
            
            await anotherMessageBus.PublishAsync(new TestIntegrationEvent(0, string.Empty));
            await channel.FinishHandlingAllMessagesAsync();
            
            Assert.True(eventHandled);
            Assert.True(anotherEventHandled);
        }
        
        [Fact]
        public async Task DoesAwaitAllPendingMessages()
        {
            var channel = new InProcMessageBusChannel();
            var messageBus = new InProcMessageBus(channel);
            messageBus.Connect();
            messageBus.ProvideInvoker(new TheMessageBus.TestInvoker());

            var allMessagesAreHandled = false;
            
            messageBus.Subscribe(new DelegateIntegrationEventHandler<TestIntegrationEvent>(x =>
            {
                if (x.StringParam == "first message")
                {
                    messageBus.PublishAsync(new TestIntegrationEvent(0, "second message"));
                }
                else if (x.StringParam == "second message")
                {
                    messageBus.PublishAsync(new TestIntegrationEvent(0, "third message"));
                }
                else if (x.StringParam == "third message")
                {
                    allMessagesAreHandled = true;
                }
            }));

            // Publish the first message and await the result.
            // This should block until all three messages are processed not only the first one was.
            await messageBus.PublishAsync(new TestIntegrationEvent(0, "first message"));
            await channel.FinishHandlingAllMessagesAsync();
            
            Assert.True(allMessagesAreHandled);
        }

        public TheInMemoryMessageBusChannel(ITestOutputHelper output) : base(output)
        {
        }
    }
}