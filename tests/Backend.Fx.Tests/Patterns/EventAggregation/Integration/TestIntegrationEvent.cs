namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    using Fx.Patterns.EventAggregation.Integration;

    public class TestIntegrationEvent : IIntegrationEvent
    {
        public int IntParam { get; }

        public string StringParam { get; }

        public TestIntegrationEvent(int intParam, string stringParam)
        {
            IntParam = intParam;
            StringParam = stringParam;
        }

        public int TenantId
        {
            get { return 55; }
        }
    }
}