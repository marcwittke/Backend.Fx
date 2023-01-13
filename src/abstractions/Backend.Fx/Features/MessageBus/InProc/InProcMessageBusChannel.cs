using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Backend.Fx.Features.MessageBus.InProc
{
    public class InProcMessageBusChannel : IAsyncDisposable
    {
        private readonly ConcurrentBag<Task> _messageHandlingTasks = new();

        private readonly ConcurrentBag<Func<SerializedMessage, Task>> _clients = new();

        internal Task PublishAsync(SerializedMessage serializedMessage)
        {
            _messageHandlingTasks.Add(Task.Factory.StartNew(() =>
            {
                foreach (var client in _clients)
                {
                    client.Invoke(serializedMessage);
                }
            }));
            
            // the async nature of the publish operation does not cover the handling, but only the publishing of the event
            return Task.CompletedTask;
        }

        internal void Connect(Func<SerializedMessage, Task> client)
        {
            _clients.Add(client);
        }

        public async Task FinishHandlingAllMessagesAsync()
        {
            await Task.WhenAll(_messageHandlingTasks).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await FinishHandlingAllMessagesAsync().ConfigureAwait(false);
        }
    }
}