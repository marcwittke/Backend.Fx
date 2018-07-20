namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using System.Threading;
    using Fx.Patterns.EventAggregation.Integration;

    public class TestIntegrationEvent : IntegrationEvent
    {
        public int IntParam { get; }

        public string StringParam { get; }

        public ManualResetEventSlim Processed = new ManualResetEventSlim(false);

        public TestIntegrationEvent(int intParam, string stringParam) : base(55)
        {
            IntParam = intParam;
            StringParam = stringParam;
        }
    }
}