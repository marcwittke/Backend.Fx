using System.Threading;
using Backend.Fx.Extensions.MessageBus;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class LongRunningEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        private readonly IIntegrationEventHandler<TestIntegrationEvent> _handler;

        public LongRunningEventHandler(IIntegrationEventHandler<TestIntegrationEvent> handler)
        {
            _handler = handler;
        }

        public void Handle(TestIntegrationEvent eventData)
        {
            Thread.Sleep(1000);
            _handler.Handle(eventData);
        }
    }
}