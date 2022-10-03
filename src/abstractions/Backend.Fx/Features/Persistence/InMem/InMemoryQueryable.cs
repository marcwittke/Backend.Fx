using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Persistence.InMem
{
    public class InMemoryQueryable<TAggregateRoot> : IQueryable<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
        private readonly IQueryable<TAggregateRoot> _aggregateQueryable;

        public InMemoryQueryable(IAggregateDictionaries aggregateDictionaries)
        {
            _aggregateQueryable = aggregateDictionaries.GetQueryable<TAggregateRoot>();
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