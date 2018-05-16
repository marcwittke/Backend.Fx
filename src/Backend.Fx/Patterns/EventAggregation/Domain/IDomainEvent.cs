namespace Backend.Fx.Patterns.EventAggregation.Domain
{
    /// <summary>
    /// Marker interface for domain events that must be handled in the same scope and transaction of the publishing logic.
    /// Handlers are called through dependency injection
    /// </summary>
    public interface IDomainEvent { }
}
