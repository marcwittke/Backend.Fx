using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Patterns.DataGeneration
{
    /// <summary>
    /// Will appear on the message bus when the data generation process has been completed
    /// </summary>
    public class DataGenerated : IntegrationEvent
    { }
}