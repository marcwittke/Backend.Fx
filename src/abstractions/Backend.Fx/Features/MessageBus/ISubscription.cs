using System;

namespace Backend.Fx.Features.MessageBus
{
    public interface ISubscription
    {
        void Process(IServiceProvider serviceProvider, EventProcessingContext context);
        bool Matches(object handler);
    }
}