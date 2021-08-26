using System.Threading;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class TestIntegrationEvent : IntegrationEvent
    {
        public TestIntegrationEvent(int intParam, string stringParam)
        {
            IntParam = intParam;
            StringParam = stringParam;
        }

        public int IntParam { get; }

        public string StringParam { get; }

        public ManualResetEventSlim TypedProcessed { get; } = new ManualResetEventSlim(false);
        public ManualResetEventSlim DynamicProcessed { get; } = new ManualResetEventSlim(false);
    }
}