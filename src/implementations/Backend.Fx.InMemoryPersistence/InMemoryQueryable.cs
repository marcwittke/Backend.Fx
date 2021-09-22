using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.InMemoryPersistence
{
    public class InMemoryQueryable<TAggregateRoot> : IQueryable<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly IQueryable<TAggregateRoot> _queryableImplementation;

        public InMemoryQueryable(IInMemoryStore<TAggregateRoot> store)
        {
            _queryableImplementation = store.Values.AsQueryable();
        }

        public IEnumerator<TAggregateRoot> GetEnumerator()
        {
            return _queryableImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_queryableImplementation).GetEnumerator();
        }

        public Type ElementType => _queryableImplementation.ElementType;

        public Expression Expression => _queryableImplementation.Expression;

        public IQueryProvider Provider => _queryableImplementation.Provider;
    }
}
