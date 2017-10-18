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
        private readonly IQueryable<TAggregateRoot> queryableImplementation;

        public InMemoryQueryable(IInMemoryStore<TAggregateRoot> store)
        {
            this.queryableImplementation = store.Values.AsQueryable();
        }


        public IEnumerator<TAggregateRoot> GetEnumerator()
        {
            return queryableImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) queryableImplementation).GetEnumerator();
        }

        public Type ElementType
        {
            get { return queryableImplementation.ElementType; }
        }

        public Expression Expression
        {
            get { return queryableImplementation.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return queryableImplementation.Provider; }
        }
    }
}