namespace Backend.Fx.Bootstrapping.Tests.DummyImpl
{
    using Patterns.EventAggregation;

    public class TheIntegrationEvent : IIntegrationEvent
    {
        public TheIntegrationEvent(int tenantId, int whatever)
        {
            TenantId = tenantId;
            Whatever = whatever;
        }
        public int TenantId { get; }
        public int Whatever { get; }
    }
}