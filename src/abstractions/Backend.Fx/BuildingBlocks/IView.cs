namespace Backend.Fx.BuildingBlocks
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public interface IView<out T> : IQueryable<T>
    {}

    public abstract class View<T> : IView<T>
    {
        private readonly IQueryable<T> _viewImplementation;

        protected View(IQueryable<T> viewImplementation)
        {
            _viewImplementation = viewImplementation;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _viewImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _viewImplementation).GetEnumerator();
        }

        public Type ElementType => _viewImplementation.ElementType;

        public Expression Expression => _viewImplementation.Expression;

        public IQueryProvider Provider => _viewImplementation.Provider;
    }
}
