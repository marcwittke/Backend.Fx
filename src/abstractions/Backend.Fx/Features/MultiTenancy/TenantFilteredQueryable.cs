using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.MultiTenancy
{
    internal class TenantFilteredQueryable<TAggregateRoot> : IQueryable<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly ITenantFilterExpressionFactory<TAggregateRoot> _tenantFilterExpressionFactory;
        private readonly IQueryable<TAggregateRoot> _queryable;

        public TenantFilteredQueryable(ITenantFilterExpressionFactory<TAggregateRoot> tenantFilterExpressionFactory, IQueryable<TAggregateRoot> queryable)
        {
            _tenantFilterExpressionFactory = tenantFilterExpressionFactory;
            _queryable = queryable;
        }

        public IEnumerator<TAggregateRoot> GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_queryable).GetEnumerator();
        }

        public Type ElementType => _queryable.ElementType;

        public Expression Expression => Expression.And(_queryable.Expression, _tenantFilterExpressionFactory.CreateTenantFilterExpression());

        public IQueryProvider Provider => _queryable.Provider;
    }
}