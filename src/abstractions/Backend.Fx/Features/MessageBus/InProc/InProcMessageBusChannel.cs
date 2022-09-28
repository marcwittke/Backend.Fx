using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Backend.Fx.Features.MessageBus.InProc
{
    public class InProcMessageBusChannel
    {
        private readonly ConcurrentBag<Task> _messageHandlingTasks = new();
        
        internal event EventHandler<MessageReceivedEventArgs> MessageReceived;

        internal void Publish(SerializedMessage serializedMessage)
        {
            var eventArgs = new MessageReceivedEventArgs { SerializedMessage = serializedMessage };
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
            public SerializedMessage SerializedMessage { get; set; }
        }
    }
}