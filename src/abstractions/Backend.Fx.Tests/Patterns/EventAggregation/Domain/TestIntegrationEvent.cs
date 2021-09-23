using Backend.Fx.Patterns.EventAggregation.Integration;
using JetBrains.Annotations;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Domain
{
    [UsedImplicitly]
    public class TestIntegrationEvent : IntegrationEvent
    {
        public TestIntegrationEvent(int whatever)
        {
            Whatever = whatever;
        }

        [UsedImplicitly]
        public int Whatever { get; }
    }
}
