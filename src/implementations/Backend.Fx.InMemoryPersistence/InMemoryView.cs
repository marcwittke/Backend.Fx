using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.InMemoryPersistence
{
    public class InMemoryView<T> : IView<T>
    {
        private readonly IList<T> _list;

        public InMemoryView(IList<T> list)
        {
            _list = list;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        public Type ElementType => typeof(T);
        public Expression Expression => _list.AsQueryable().Expression;
        public IQueryProvider Provider => _list.AsQueryable().Provider;
    }
}