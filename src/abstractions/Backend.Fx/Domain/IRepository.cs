using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.Domain
{
    /// <summary>
    /// Encapsulates methods for retrieving domain objects 
    /// See https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    [PublicAPI]
    public interface IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        Task<TAggregateRoot> SingleAsync(int id, CancellationToken cancellationToken = default);

        Task<TAggregateRoot> SingleOrDefaultAsync(int id, CancellationToken cancellationToken = default);

        Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(CancellationToken cancellationToken = default);

        Task<TAggregateRoot[]> ResolveAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

        Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default);

        Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default);

        Task AddRangeAsync(TAggregateRoot[] aggregateRoots, CancellationToken cancellationToken = default);
    }

    public abstract class Repository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly IAsyncAggregateQueryable<TAggregateRoot> _queryable;

        protected Repository(IAsyncAggregateQueryable<TAggregateRoot> queryable)
        {
            _queryable = queryable;
        }

        public async Task<TAggregateRoot> SingleAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _queryable.SingleAsync(ar => ar.Id == id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TAggregateRoot> SingleOrDefaultAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _queryable.SingleOrDefaultAsync(ar => ar.Id == id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _queryable.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await _queryable.AnyAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TAggregateRoot[]> ResolveAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken = default)
        {
            var idArray = ids as int[] ?? ids.ToArray();
            var resolved = new TAggregateRoot[idArray.Length];
            for (var i = 0; i < idArray.Length; i++)
            {
                resolved[i] = await SingleAsync(idArray[i], cancellationToken).ConfigureAwait(false);
            }

            return resolved;
        }

        public abstract Task DeleteAsync(TAggregateRoot aggregateRoot,CancellationToken cancellationToken = default);

        public abstract Task AddAsync(TAggregateRoot aggregateRoot,CancellationToken cancellationToken = default);

        public abstract Task AddRangeAsync(TAggregateRoot[] aggregateRoots, CancellationToken cancellationToken = default);
    }
}