namespace Backend.Fx.Tests.Patterns.EventAggregation.Domain
{
    using Fx.Patterns.EventAggregation.Integration;

    public class TestIntegrationEvent : IntegrationEvent
    {
        public int Whatever { get; }

        public TestIntegrationEvent(int whatever, int tenantId) : base(tenantId)
        {
            Whatever = whatever;
        }
    }
}