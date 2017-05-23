namespace Backend.Fx.Patterns.EventAggregation
{
    /// <summary>
    /// Marker interface for events that should be handled in a separate scope and transaction. Might be persisted as well.
    /// Handlers must be explicitly registered using the application runtime.
    /// See https://blogs.msdn.microsoft.com/cesardelatorre/2017/02/07/domain-events-vs-integration-events-in-domain-driven-design-and-microservices-architectures/
    /// </summary>
    public interface IIntegrationEvent {
        int TenantId { get; }
    }
}
