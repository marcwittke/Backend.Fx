using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Persistence
{
    public interface IAggregateQueryable<out TAggregateRoot, TId> : IQueryable<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot<TId>
        where TId : struct, IEquatable<TId>
    {
    }

    public class AggregateQueryable<TAggregateRoot, TId> : IAggregateQueryable<TAggregateRoot, TId>
        where TAggregateRoot : IAggregateRoot<TId>
        where TId : struct, IEquatable<TId>
    {
        private readonly IQueryable<TAggregateRoot> _queryable;

        public AggregateQueryable(IQueryable<TAggregateRoot> queryable)
        {
            _queryable = queryable;
        }

        public IEnumerator<TAggregateRoot> GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        public Type ElementType => _queryable.ElementType;

        public Expression Expression => _queryable.Expression;

        public IQueryProvider Provider => _queryable.Provider;
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}