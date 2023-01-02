using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;

namespace Backend.Fx.Features.Persistence.InMem
{
    [PublicAPI]
    public class InMemoryRepository<TAggregateRoot, TId> : IRepository<TAggregateRoot, TId>
        where TAggregateRoot : class, IAggregateRoot<TId>
        where TId : IEquatable<TId>
    {
        private readonly IQueryable<TAggregateRoot> _aggregateQueryable;

        public InMemoryRepository(
            IAggregateDictionaries<TId> aggregateDictionaries,
            IQueryable<TAggregateRoot> aggregateQueryable)
        {
            // we could get the queryable directly from the store, but that would prevent the DI from decorating
            // the queryable, as it is done e.g. in case of Authorization 
            _aggregateQueryable = aggregateQueryable;
            Store = aggregateDictionaries.For<TAggregateRoot>();
        }

        public virtual IAggregateDictionary<TAggregateRoot, TId> Store { get; }

        public void Clear()
        {
            Store.Clear();
        }

        public Task<TAggregateRoot> GetAsync(TId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_aggregateQueryable.FirstOrDefault(agg => Equals(agg.Id, id))
                                   ?? throw new NotFoundException<TAggregateRoot>(id));
        }

        public Task<TAggregateRoot> FindAsync(TId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_aggregateQueryable.FirstOrDefault(agg => Equals(agg.Id, id)));
        }

        public Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // the "useless" where condition makes sure we do not return the underlying array, but evaluate the 
            // expression, that might have been extended with additional filters (authorization, multi tenancy)
            return Task.FromResult(_aggregateQueryable.Where(agg => true).ToArray());
        }

        public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_aggregateQueryable.Any());
        }

        public Task<TAggregateRoot[]> ResolveAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
        {
            return RepositoryEx.ResolveAsync(this, ids, cancellationToken);
        }

        public Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
        {
            if (Store.ContainsKey(aggregateRoot.Id))
            {
                Store.Remove(aggregateRoot.Id);
            }

            return Task.CompletedTask;
        }

        public Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
        {
            if (Store.ContainsKey(aggregateRoot.Id))
            {
                throw new ConflictedException(
                    $"There is already an {aggregateRoot.GetType().Name} with id {aggregateRoot.Id} present");
            }

            Store[aggregateRoot.Id] = aggregateRoot;
            return Task.CompletedTask;
        }

        public async Task AddRangeAsync(TAggregateRoot[] aggregateRoots,
            CancellationToken cancellationToken = default)
        {
            foreach (TAggregateRoot aggregateRoot in aggregateRoots)
            {
                await AddAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}