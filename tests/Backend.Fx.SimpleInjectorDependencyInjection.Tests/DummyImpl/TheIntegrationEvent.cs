using Backend.Fx.Patterns.EventAggregation.Integration;
using JetBrains.Annotations;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl
{
    [UsedImplicitly]
    public class TheIntegrationEvent : IntegrationEvent
    {
        public TheIntegrationEvent(int tenantId, int whatever) : base(tenantId)
        {
            Whatever = whatever;
        }

        public int Whatever { get; }
    }
}