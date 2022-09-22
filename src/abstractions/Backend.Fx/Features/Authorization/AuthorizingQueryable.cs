using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Authorization
{
    /// <summary>
    /// Applies the authorization policy expression to the queryable via decoration
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    internal class AuthorizingQueryable<TAggregateRoot> : IAsyncAggregateQueryable<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly AuthorizationPolicy<TAggregateRoot> _authorizationPolicy;
        private readonly IAsyncAggregateQueryable<TAggregateRoot> _aggregateQueryable;

        public AuthorizingQueryable(AuthorizationPolicy<TAggregateRoot> authorizationPolicy, IAsyncAggregateQueryable<TAggregateRoot> aggregateQueryable)
        {
            _authorizationPolicy = authorizationPolicy;
            _aggregateQueryable = aggregateQueryable;
        }

        public Type ElementType => _aggregateQueryable.ElementType;

        public Expression Expression => Expression.And(_aggregateQueryable.Expression, _authorizationPolicy.HasAccessExpression);

        IAsyncQueryProvider IAsyncQueryable.Provider => _aggregateQueryable.Provider;

        public IAsyncEnumerator<TAggregateRoot> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return _aggregateQueryable.GetAsyncEnumerator(cancellationToken);
        }
    }
}