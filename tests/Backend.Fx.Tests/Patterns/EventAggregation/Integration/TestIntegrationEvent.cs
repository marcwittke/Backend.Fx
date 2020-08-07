namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System.Threading;
    using Backend.Fx.Patterns.EventAggregation.Integration;
    
    public class TestIntegrationEvent : IntegrationEvent
    {
        public int IntParam { get; }

        public string StringParam { get; }

        public ManualResetEventSlim TypedProcessed { get; } = new ManualResetEventSlim(false);
        public ManualResetEventSlim DynamicProcessed { get; } = new ManualResetEventSlim(false);

        public TestIntegrationEvent(int intParam, string stringParam) : base(55)
        {
            IntParam = intParam;
            StringParam = stringParam;
        }
    }
}