namespace Backend.Fx.Features.MessageBus
{
    public struct SerializedMessage
    {
        public SerializedMessage(string eventTypeName, byte[] messagePayload)
        {
            EventTypeName = eventTypeName;
            MessagePayload = messagePayload;
        }

        public string EventTypeName { get; }
        public byte[] MessagePayload { get; }
    }
}
