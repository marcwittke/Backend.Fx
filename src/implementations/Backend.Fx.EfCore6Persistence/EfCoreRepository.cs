using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;
using Backend.Fx.Exceptions;
using Backend.Fx.Features.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence;

public class EfCoreRepository<TAggregateRoot, TId> : IRepository<TAggregateRoot, TId>
    where TAggregateRoot : class, IAggregateRoot<TId>
    where TId : struct, IEquatable<TId>

{
    private readonly DbContext _dbContext;
    private readonly IQueryable<TAggregateRoot> _queryable;

    public EfCoreRepository(DbContext dbContext, IQueryable<TAggregateRoot> queryable)
    {
        _dbContext = dbContext;
        _queryable = queryable;
    }

    public async Task<TAggregateRoot> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        return (await _queryable
                   .FirstOrDefaultAsync(agg => Equals(agg.Id, id), cancellationToken: cancellationToken)
                   .ConfigureAwait(false))
               ?? throw new NotFoundException<TAggregateRoot>(id);
    }

    public async Task<TAggregateRoot> FindAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _queryable
            .FirstOrDefaultAsync(agg => Equals(agg.Id, id), cancellationToken: cancellationToken)
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

    public Task<TAggregateRoot[]> ResolveAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
    {
        return RepositoryEx.ResolveAsync(this, ids, cancellationToken);
    }

    public Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TAggregateRoot>().Remove(aggregateRoot);
        return Task.CompletedTask;
    }

    public async Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TAggregateRoot>().AddAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
    }

    public async Task AddRangeAsync(
        TAggregateRoot[] aggregateRoots,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TAggregateRoot>().AddRangeAsync(aggregateRoots, cancellationToken).ConfigureAwait(false);
    }
}