namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildingBlocks;

    public class InMemoryQueryable<TAggregateRoot> : IQueryable<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly IQueryable<TAggregateRoot> _queryableImplementation;

        public InMemoryQueryable(IInMemoryStore<TAggregateRoot> store)
        {
            this._queryableImplementation = store.Values.AsQueryable();
        }


        public IEnumerator<TAggregateRoot> GetEnumerator()
        {
            return _queryableImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _queryableImplementation).GetEnumerator();
        }

        public Type ElementType
        {
            get { return _queryableImplementation.ElementType; }
        }

        public Expression Expression
        {
            get { return _queryableImplementation.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return _queryableImplementation.Provider; }
        }
    }
}