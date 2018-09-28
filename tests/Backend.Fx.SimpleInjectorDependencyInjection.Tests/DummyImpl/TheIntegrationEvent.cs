using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl
{
    public class TheIntegrationEvent : IntegrationEvent
    {
        public TheIntegrationEvent(int tenantId, int whatever) : base(tenantId)
        {
            Whatever = whatever;
        }

        public int Whatever { get; }
    }
}