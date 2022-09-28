using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Persistence.InMem
{
    public class InMemoryAggregateQueryable<TAggregateRoot, TId> : IAggregateQueryable<TAggregateRoot, TId>
        where TId : struct, IEquatable<TId>
        where TAggregateRoot : IAggregateRoot<TId>
    {
        private readonly AggregateQueryable<TAggregateRoot, TId> _aggregateQueryable;

        public InMemoryAggregateQueryable(IAggregateDictionaries aggregateDictionaries)
        {
            _aggregateQueryable = new AggregateQueryable<TAggregateRoot, TId>(
                aggregateDictionaries.Get<TAggregateRoot, TId>().AsQueryable().Select(kvp => kvp.Value));
        }

        public Type ElementType => _aggregateQueryable.ElementType;

        public Expression Expression => _aggregateQueryable.Expression;
        
        IQueryProvider IQueryable.Provider => _aggregateQueryable.Provider;

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