using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Patterns.DataGeneration
{
    public class DataGenerated : IntegrationEvent
    {
        public DataGenerated(int tenantId) : base(tenantId)
        {
        }
    }
}