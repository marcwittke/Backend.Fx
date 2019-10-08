namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System.Threading;
    using Fx.Patterns.EventAggregation.Integration;

    public class LongRunningEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        public void Handle(TestIntegrationEvent eventData)
        {
            Thread.Sleep(1000);
        }
    }}
