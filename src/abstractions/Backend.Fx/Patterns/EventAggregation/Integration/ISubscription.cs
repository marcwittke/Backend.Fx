using System;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface ISubscription
    {
        void Process(IServiceProvider serviceProvider, EventProcessingContext context);
        bool Matches(object handler);
    }
}