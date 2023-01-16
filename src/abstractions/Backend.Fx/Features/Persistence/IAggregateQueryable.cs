using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Persistence
{
    public interface IAggregateQueryable<TAggregateRoot, in TId>
        where TAggregateRoot : class, IAggregateRoot<TId>
        where TId : IEquatable<TId>
    {
        Task<TAggregateRoot> GetAsync(TId id, CancellationToken cancellationToken = default);
        Task<TAggregateRoot> FindAsync(TId id, CancellationToken cancellationToken = default);
        Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    }
}