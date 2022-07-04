using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Backend.Fx.Features.MessageBus
{
    public class InMemoryMessageBusChannel
    {
        private readonly ConcurrentBag<Task> _messageHandlingTasks = new ConcurrentBag<Task>();
        
        internal event EventHandler<MessageReceivedEventArgs> MessageReceived;

        internal void Publish(IIntegrationEvent integrationEvent)
        {
            var eventArgs = new MessageReceivedEventArgs { IntegrationEvent = integrationEvent };
            _messageHandlingTasks.Add(Task.Run(() => MessageReceived?.Invoke(this, eventArgs)));
        }

        public async Task FinishHandlingAllMessagesAsync()
        {
            while (_messageHandlingTasks.TryTake(out var messageHandlingTask))
            {
                await messageHandlingTask.ConfigureAwait(false);
            }
        }
        
        internal class MessageReceivedEventArgs
        {
            public IIntegrationEvent IntegrationEvent { get; set; }
        }
    }
}