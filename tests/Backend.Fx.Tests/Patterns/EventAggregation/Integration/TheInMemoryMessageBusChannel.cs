using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Xunit;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class TheInMemoryMessageBusChannel
    {
        [Fact]
        public async Task HandlesEventsAsynchronously()
        {
            var channel = new InMemoryMessageBusChannel();
            var messageBus = new InMemoryMessageBus(channel);
            messageBus.Connect();
            messageBus.ProvideInvoker(new TheMessageBus.TestInvoker());

            var handled = new ManualResetEvent(false);
            messageBus.Subscribe(new BlockingMessageHandler(handled));

            await messageBus.Publish(new TestIntegrationEvent(0, string.Empty));

            var finishHandleTask = channel.FinishHandlingAllMessagesAsync();
            Assert.Contains(finishHandleTask.Status, new[] { TaskStatus.WaitingForActivation, TaskStatus.Running });
            handled.Set();

            await finishHandleTask;
        }

        [Fact]
        public async Task InvokesAllApplicationHandlers()
        {
            var channel = new InMemoryMessageBusChannel();

            var messageBus = new InMemoryMessageBus(channel);
            var eventHandled = false;
            messageBus.Connect();
            messageBus.ProvideInvoker(new TheMessageBus.TestInvoker());
            messageBus.Subscribe(
                new DelegateIntegrationMessageHandler<TestIntegrationEvent>(ev => eventHandled = true));

            var anotherMessageBus = new InMemoryMessageBus(channel);
            var anotherEventHandled = false;
            anotherMessageBus.Connect();
            anotherMessageBus.ProvideInvoker(new TheMessageBus.TestInvoker());
            messageBus.Subscribe(
                new DelegateIntegrationMessageHandler<TestIntegrationEvent>(ev => anotherEventHandled = true));

            await messageBus.Publish(new TestIntegrationEvent(0, string.Empty));
            await channel.FinishHandlingAllMessagesAsync();

            Assert.True(eventHandled);
            Assert.True(anotherEventHandled);

            eventHandled = false;
            anotherEventHandled = false;

            await anotherMessageBus.Publish(new TestIntegrationEvent(0, string.Empty));
            await channel.FinishHandlingAllMessagesAsync();

            Assert.True(eventHandled);
            Assert.True(anotherEventHandled);
        }

        [Fact]
        public async Task DoesAwaitAllPendingMessages()
        {
            var channel = new InMemoryMessageBusChannel();
            var messageBus = new InMemoryMessageBus(channel);
            messageBus.Connect();
            messageBus.ProvideInvoker(new TheMessageBus.TestInvoker());

            var allMessagesAreHandled = false;

            messageBus.Subscribe(
                new DelegateIntegrationMessageHandler<TestIntegrationEvent>(
                    x =>
                    {
                        if (x.StringParam == "first message")
                        {
                            messageBus.Publish(new TestIntegrationEvent(0, "second message"));
                        }
                        else if (x.StringParam == "second message")
                        {
                            messageBus.Publish(new TestIntegrationEvent(0, "third message"));
                        }
                        else if (x.StringParam == "third message")
                        {
                            allMessagesAreHandled = true;
                        }
                    }));

            // Publish the first message and await the result.
            // This should block until all three messages are processed not only the first one was.
            await messageBus.Publish(new TestIntegrationEvent(0, "first message"));
            await channel.FinishHandlingAllMessagesAsync();

            Assert.True(allMessagesAreHandled);
        }
    }
}
