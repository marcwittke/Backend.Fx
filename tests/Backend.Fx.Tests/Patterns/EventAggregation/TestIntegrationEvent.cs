namespace Backend.Fx.Tests.Patterns.EventAggregation
{
    using Fx.Patterns.EventAggregation;

    public class TestIntegrationEvent : IIntegrationEvent
    {
        public TestIntegrationEvent(int id, int tenantId)
        {
            Id = id;
            TenantId = tenantId;
        }

        public int Id { get; }
        public int TenantId { get; }
    }
}