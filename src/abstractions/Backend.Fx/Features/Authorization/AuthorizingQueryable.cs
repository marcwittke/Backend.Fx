using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Authorization
{
    /// <summary>
    /// Applies the authorization policy expression to the queryable via decoration
    /// </summary>
    internal class AuthorizingQueryable<TAggregateRoot> : IQueryable<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
        private readonly IAuthorizationPolicy<TAggregateRoot> _authorizationPolicy;
        private readonly IQueryable<TAggregateRoot> _aggregateQueryable;

        public AuthorizingQueryable(IAuthorizationPolicy<TAggregateRoot> authorizationPolicy, IQueryable<TAggregateRoot> aggregateQueryable)
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