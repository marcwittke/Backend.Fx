namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface ISubscription
    {
        void Process(string eventName, EventProcessingContext context);
        bool Matches(object handler);
    }
}