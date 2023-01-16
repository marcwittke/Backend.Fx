using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Domain;
using Backend.Fx.Exceptions;
using Backend.Fx.Features.Persistence;

namespace Backend.Fx.Features.Authorization
{
    /// <summary>
    /// Checks the authorization policy for write operations
    /// </summary>
    internal class AuthorizingRepository<TAggregateRoot, TId> : IRepository<TAggregateRoot, TId>
        where TAggregateRoot : class, IAggregateRoot<TId> 
        where TId : IEquatable<TId>
    {
        private readonly IAuthorizationPolicy<TAggregateRoot> _authorizationPolicy;
        private readonly IRepository<TAggregateRoot, TId> _repository;

        public AuthorizingRepository(IAuthorizationPolicy<TAggregateRoot> authorizationPolicy, IRepository<TAggregateRoot, TId> repository)
        {
            _authorizationPolicy = authorizationPolicy;
            _repository = repository;
        }

        public Task<TAggregateRoot> GetAsync(TId id, CancellationToken cancellationToken = default)
        {
            return _repository.GetAsync(id, cancellationToken);
        }

        public Task<TAggregateRoot> FindAsync(TId id, CancellationToken cancellationToken = default)
        {
            return _repository.FindAsync(id, cancellationToken);
        }

        public Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetAllAsync(cancellationToken);
        }

        public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return _repository.AnyAsync(cancellationToken);
        }

        public Task<TAggregateRoot[]> ResolveAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
        {
            return _repository.ResolveAsync(ids, cancellationToken);
        }

        public async Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
        {
            if (_authorizationPolicy.CanDelete(aggregateRoot))
            {
                await _repository.DeleteAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new ForbiddenException("You are not allowed to delete this record");
            }
        }

        public async Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
        {
            if (_authorizationPolicy.CanCreate(aggregateRoot))
            {
                await _repository.AddAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new ForbiddenException("You are not allowed to create such a record");
            }
        }

        public async Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
        {
            if (_authorizationPolicy.CanModify(aggregateRoot))
            {
                await _repository.AddAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new ForbiddenException("You are not allowed to create such a record");
            }
        }

        public async Task AddRangeAsync(TAggregateRoot[] aggregateRoots, CancellationToken cancellationToken = default)
        {
            if (aggregateRoots.Any(ar => !_authorizationPolicy.CanCreate(ar)))
            {
                throw new ForbiddenException("You are not allowed to create such a record");
            }
            
            await _repository.AddRangeAsync(aggregateRoots, cancellationToken).ConfigureAwait(false);
        }
    }
}