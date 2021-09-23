using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface ISubscription
    {
        void Process(IInstanceProvider instanceProvider, EventProcessingContext context);
        bool Matches(object handler);
    }
}
