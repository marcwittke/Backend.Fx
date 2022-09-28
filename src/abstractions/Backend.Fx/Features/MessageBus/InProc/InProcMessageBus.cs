using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Fx.Features.MessageBus.InProc
{
    public class InProcMessageBus : MessageBus
    {
        private readonly InProcMessageBusChannel _channel;
        private readonly HashSet<string> _subscribedEventTypeNames = new();
        
        public InProcMessageBus()
        {
            _channel = new InProcMessageBusChannel();
        }

        public override void Connect()
        {
            _channel.MessageReceived += ChannelOnMessageReceived;
        }

        protected override void SubscribeToEventMessage(string eventTypeName)
        {
            _subscribedEventTypeNames.Add(eventTypeName);
        }

        protected override Task PublishMessageAsync(SerializedMessage serializedMessage)
        {
            _channel.Publish(serializedMessage);
            return Task.CompletedTask;
        }

        private async void ChannelOnMessageReceived(
            object sender, 
            InProcMessageBusChannel.MessageReceivedEventArgs eventArgs)
        {
            if (_subscribedEventTypeNames.Contains(eventArgs.SerializedMessage.EventTypeName))
            {
                await ProcessAsync(eventArgs.SerializedMessage).ConfigureAwait(false);
            }
        }
    }
}