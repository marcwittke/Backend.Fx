namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface IIntegrationMessageHandler
    {
        void Handle(dynamic eventData);
    }


    public interface IIntegrationMessageHandler<in TEvent> where TEvent : IIntegrationEvent
    {
        void Handle(TEvent eventData);
    }
}
