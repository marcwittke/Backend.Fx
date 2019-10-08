using System.Threading.Tasks;
using Backend.Fx.Patterns.EventAggregation.Domain;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain
{
    public class ADomainEventHandler2 : IDomainEventHandler<ADomainEvent>
    {
        public Task HandleAsync(ADomainEvent domainEvent)
        {
            return Task.CompletedTask;
        }
    }
}