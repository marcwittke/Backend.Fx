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
    public interface IRepository<TAggregateRoot, in TId>
        where TAggregateRoot : class, IAggregateRoot<TId> 
        where TId : IEquatable<TId>
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
        
        Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default);

        Task AddRangeAsync(TAggregateRoot[] aggregateRoots, CancellationToken cancellationToken = default);
    }

    public class Repository<TAggregateRoot, TId> : IRepository<TAggregateRoot, TId>
        where TAggregateRoot : class, IAggregateRoot<TId>
        where TId : IEquatable<TId>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAggregateQueryable<TAggregateRoot, TId> _queryable;

        protected Repository(IUnitOfWork unitOfWork, IAggregateQueryable<TAggregateRoot, TId> queryable)
        {
            _unitOfWork = unitOfWork;
            _queryable = queryable;
        }

        public async Task<TAggregateRoot> GetAsync(TId id, CancellationToken cancellationToken = default) =>
            await _queryable.GetAsync(id, cancellationToken).ConfigureAwait(false);
        
        public async Task<TAggregateRoot> FindAsync(TId id, CancellationToken cancellationToken = default)=>
            await _queryable.FindAsync(id, cancellationToken).ConfigureAwait(false);
        
        public async Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default)=>
            await _queryable.GetAllAsync(cancellationToken).ConfigureAwait(false);
        
        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)=>
            await _queryable.AnyAsync(cancellationToken).ConfigureAwait(false);

        public async Task<TAggregateRoot[]> ResolveAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
        {
            var idArray = ids as TId[] ?? ids.ToArray();
            var resolved = new TAggregateRoot[idArray.Length];
            using IExceptionBuilder builder = NotFoundException.UseBuilder();
            for (var i = 0; i < idArray.Length; i++)
            {
                resolved[i] = await FindAsync(idArray[i], cancellationToken).ConfigureAwait(false);
                builder.AddNotFoundWhenNull(idArray[i], resolved[i]);
            }

            return resolved;
        }

        public async Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default) => 
            await _unitOfWork.RegisterDeletedAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);

        public async Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default) => 
            await _unitOfWork.RegisterNewAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);

        public async Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default) => 
            await _unitOfWork.RegisterDirtyAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);

        public async Task AddRangeAsync(TAggregateRoot[] aggregateRoots, CancellationToken cancellationToken = default) =>
            await _unitOfWork.RegisterDirtyAsync(aggregateRoots.Cast<IAggregateRoot>().ToArray(), cancellationToken).ConfigureAwait(false);
    }
}