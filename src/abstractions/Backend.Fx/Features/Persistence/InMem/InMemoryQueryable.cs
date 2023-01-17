using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;
using Backend.Fx.Exceptions;

namespace Backend.Fx.Features.Persistence.InMem
{
    public class InMemoryQueryable<TAggregateRoot, TId> : IAggregateQueryable<TAggregateRoot, TId>
        where TAggregateRoot : class, IAggregateRoot<TId> 
        where TId : IEquatable<TId>
    {
        private readonly IAggregateQueryable<TAggregateRoot, TId> _aggregateQueryable;

        public InMemoryQueryable(IAggregateDictionaries aggregateDictionaries)
        {
            _aggregateQueryable = aggregateDictionaries.GetQueryable<TAggregateRoot, TId>();
        }

        public Task<TAggregateRoot> GetAsync(TId id, CancellationToken cancellationToken = default)
        {
            return _aggregateQueryable.GetAsync(id, cancellationToken) ?? throw new NotFoundException<TAggregateRoot>(id);
        }

        public Task<TAggregateRoot> FindAsync(TId id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}