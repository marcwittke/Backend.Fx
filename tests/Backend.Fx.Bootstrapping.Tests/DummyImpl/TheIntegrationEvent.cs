namespace Backend.Fx.Bootstrapping.Tests.DummyImpl
{
    using Patterns.EventAggregation.Integration;

    public class TheIntegrationEvent : IntegrationEvent
    {
        public TheIntegrationEvent(int tenantId, int whatever) : base(tenantId)
        {
            Whatever = whatever;
        }

        public int Whatever { get; }
    }
}