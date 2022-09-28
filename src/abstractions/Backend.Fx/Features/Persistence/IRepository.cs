using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;

namespace Backend.Fx.Features.Persistence
{
    /// <summary>
    /// Encapsulates methods for retrieving domain objects 
    /// See https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    [PublicAPI]
    public interface IRepository<TAggregateRoot, in TId> where TAggregateRoot : IAggregateRoot<TId> where TId : struct, IEquatable<TId>
    {
        /// <summary>
        /// Throws a <see cref="NotFoundException{TAggregateRoot}"/> when nothing matches the given id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TAggregateRoot> GetAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns <c>null</c> when nothing matches the given id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TAggregateRoot> FindAsync(TId id, CancellationToken cancellationToken = default);

        Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(CancellationToken cancellationToken = default);

        Task<TAggregateRoot[]> ResolveAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);

        Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default);

        Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default);

        Task AddRangeAsync(TAggregateRoot[] aggregateRoots, CancellationToken cancellationToken = default);
    }
    
    public static class RepositoryEx
    {
        public static async Task<TAggregateRoot[]> ResolveAsync<TAggregateRoot, TId>(
            this IRepository<TAggregateRoot, TId> repository,
            IEnumerable<TId> ids,
            CancellationToken cancellationToken = default)
            where TAggregateRoot : IAggregateRoot<TId>
            where TId : struct, IEquatable<TId>
        {
            var idArray = ids as TId[] ?? ids.ToArray();
            var resolved = new TAggregateRoot[idArray.Length];
            for (var i = 0; i < idArray.Length; i++)
            {
                resolved[i] = await repository.GetAsync(idArray[i], cancellationToken).ConfigureAwait(false);
            }

            return resolved;
        }
    }
}