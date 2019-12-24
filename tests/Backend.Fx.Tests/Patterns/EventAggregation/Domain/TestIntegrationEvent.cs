using JetBrains.Annotations;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Domain
{
    using Fx.Patterns.EventAggregation.Integration;

    [UsedImplicitly]
    public class TestIntegrationEvent : IntegrationEvent
    {
        [UsedImplicitly]
        public int Whatever { get; }

        public TestIntegrationEvent(int whatever, int tenantId) : base(tenantId)
        {
            Whatever = whatever;
        }
    }
}