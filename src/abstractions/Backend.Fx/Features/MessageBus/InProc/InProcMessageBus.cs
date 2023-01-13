using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Fx.Features.MessageBus.InProc
{
    public class InProcMessageBus : MessageBus
    {
        private readonly InProcMessageBusChannel _channel;
        private readonly HashSet<string> _subscribedEventTypeNames = new();
        
        public InProcMessageBus(InProcMessageBusChannel channel)
        {
            _channel = channel;
        }

        public override void Connect()
        {
            _channel.Connect(async msg =>
            {
                if (_subscribedEventTypeNames.Contains(msg.EventTypeName))
                {
                    await ProcessAsync(msg).ConfigureAwait(false);
                }    
            });
        }

        protected override void SubscribeToEventMessage(string eventTypeName)
        {
            _subscribedEventTypeNames.Add(eventTypeName);
        }

        protected override async Task PublishMessageAsync(SerializedMessage serializedMessage)
        {
            await _channel.PublishAsync(serializedMessage).ConfigureAwait(false);
        }
    }
}