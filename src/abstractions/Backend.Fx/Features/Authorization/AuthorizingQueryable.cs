using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.Domain;
using Backend.Fx.Features.Persistence;

namespace Backend.Fx.Features.Authorization
{
    /// <summary>
    /// Applies the authorization policy expression to the queryable via decoration
    /// </summary>
    internal class AuthorizingQueryable<TAggregateRoot, TId> : IAggregateQueryable<TAggregateRoot, TId>
        where TAggregateRoot : IAggregateRoot<TId> 
        where TId : struct, IEquatable<TId>
    {
        private readonly IAuthorizationPolicy<TAggregateRoot, TId> _authorizationPolicy;
        private readonly IAggregateQueryable<TAggregateRoot, TId> _aggregateQueryable;

        public AuthorizingQueryable(IAuthorizationPolicy<TAggregateRoot, TId> authorizationPolicy, IAggregateQueryable<TAggregateRoot, TId> aggregateQueryable)
        {
            _authorizationPolicy = authorizationPolicy;
            _aggregateQueryable = aggregateQueryable;
        }

        public Type ElementType => _aggregateQueryable.ElementType;

        public Expression Expression
        {
            get
            {
                // expression tree manipulation: apply the HasAccessExpression to the basic Queryable Expression using "Where"
                MethodCallExpression queryableWhereExpression = Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new [] { ElementType },
                    _aggregateQueryable.Expression, 
                    Expression.Quote(_authorizationPolicy.HasAccessExpression));

                return queryableWhereExpression;
            }
        }

        public IQueryProvider Provider => _aggregateQueryable.Provider;

        public IEnumerator<TAggregateRoot> GetEnumerator()
        {
            return _aggregateQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_aggregateQueryable).GetEnumerator();
        }
    }
}