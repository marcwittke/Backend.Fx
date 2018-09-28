namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface IIntegrationEventHandler
    {
        void Handle(dynamic eventData);
    }

    public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
    {
        void Handle(TEvent eventData);
    }
}
