namespace Backend.Fx.Patterns.EventAggregation
{
    using System.Threading.Tasks;

    public interface IAsyncDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
    {
        Task HandleAsync(TDomainEvent domainEvent);
    }
}