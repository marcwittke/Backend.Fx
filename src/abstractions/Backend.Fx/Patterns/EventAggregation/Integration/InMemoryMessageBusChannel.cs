namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    public class InMemoryMessageBusChannel
    {
        private readonly List<Task> _messageHandlingTasks = new List<Task>();
        
        internal event EventHandler<MessageReceivedEventArgs> MessageReceived;

        internal void Publish(IIntegrationEvent integrationEvent)
        {
            var eventArgs = new MessageReceivedEventArgs { IntegrationEvent = integrationEvent };
            _messageHandlingTasks.Add(Task.Run(() => MessageReceived?.Invoke(this, eventArgs)));
        }

        public async Task FinishHandlingAllMessagesAsync()
        {
            await Task.WhenAll(_messageHandlingTasks);
            _messageHandlingTasks.Clear();
        }
        
        internal class MessageReceivedEventArgs
        {
            public IIntegrationEvent IntegrationEvent { get; set; }
        }
    }
}