using Backend.Fx.Patterns.EventAggregation.Integration;
using JetBrains.Annotations;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain
{
    [UsedImplicitly]
    public class AnIntegrationEvent : IntegrationEvent
    {
        public AnIntegrationEvent(int tenantId, int whatever) : base(tenantId)
        {
            Whatever = whatever;
        }

        public int Whatever { [UsedImplicitly] get; }
    }
}
