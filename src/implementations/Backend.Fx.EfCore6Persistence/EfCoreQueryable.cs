using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;
using Backend.Fx.Exceptions;
using Backend.Fx.Features.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence;

public class EfCoreQueryable<TAggregateRoot, TId> : IAggregateQueryable<TAggregateRoot, TId>
    where TAggregateRoot : class, IAggregateRoot<TId>
    where TId : IEquatable<TId>
{
    private readonly IQueryable<TAggregateRoot> _queryable;
    
    public EfCoreQueryable(DbContext dbContext)
    {
        _queryable = dbContext.Set<TAggregateRoot>();
    }

    public async Task<TAggregateRoot> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _queryable
            .SingleAsync(agg => agg.Id.Equals(id), cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException<TAggregateRoot>(id);
    }

    public async Task<TAggregateRoot> FindAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _queryable
            .SingleOrDefaultAsync(agg => agg.Id.Equals(id), cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _queryable
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await _queryable
            .AnyAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}