using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Persistence
{
    public interface IUnitOfWork
    {
        Task RegisterNewAsync(IAggregateRoot aggregateRoot, CancellationToken cancellation);
        
        Task RegisterDirtyAsync(IAggregateRoot aggregateRoot, CancellationToken cancellation);
        
        Task RegisterDeletedAsync(IAggregateRoot aggregateRoot, CancellationToken cancellation);
        
        Task RegisterDirtyAsync(IAggregateRoot[] aggregateRoots, CancellationToken cancellationToken);
    }
}