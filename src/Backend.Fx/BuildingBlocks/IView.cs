namespace Backend.Fx.BuildingBlocks
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public interface IView<out T> : IQueryable<T>, IDomainService
    {}

    public abstract class View<T> : IView<T>
    {
        private readonly IQueryable<T> viewImplementation;

        protected View(IQueryable<T> viewImplementation)
        {
            this.viewImplementation = viewImplementation;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return viewImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) viewImplementation).GetEnumerator();
        }

        public Type ElementType
        {
            get { return viewImplementation.ElementType; }
        }

        public Expression Expression
        {
            get { return viewImplementation.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return viewImplementation.Provider; }
        }
    }
}
