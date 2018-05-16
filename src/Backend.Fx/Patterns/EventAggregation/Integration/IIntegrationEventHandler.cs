namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System.Threading.Tasks;

    public interface IIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
