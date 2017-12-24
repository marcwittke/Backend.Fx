namespace Backend.Fx.Patterns.Authorization
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildingBlocks;

    public abstract class AggregateAuthorization<TAggregateRoot> : IAggregateAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        /// <inheritdoc />>
        public virtual Expression<Func<TAggregateRoot, bool>> HasAccessExpression
        {
            get { return agg => true; }
        }

        /// <inheritdoc />>
        public virtual IQueryable<TAggregateRoot> Filter(IQueryable<TAggregateRoot> queryable)
        {
            return queryable.Where(HasAccessExpression);
        }

        /// <inheritdoc />>
        public virtual bool CanCreate(TAggregateRoot t)
        {
            return true;
        }

        /// <inheritdoc />>
        public virtual bool CanModify(TAggregateRoot t)
        {
            return true;
        }

        public bool CanDelete(TAggregateRoot t)
        {
            return CanModify(t);
        }
    }
}