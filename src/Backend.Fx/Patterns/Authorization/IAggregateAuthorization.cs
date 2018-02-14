namespace Backend.Fx.Patterns.Authorization
{
    using System.Linq;
    using System.Linq.Expressions;
    using BuildingBlocks;

    public interface IAggregateAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        /// <summary>
        /// Express a filter for repository queryable
        /// </summary>
        Expression<System.Func<TAggregateRoot, bool>> HasAccessExpression { get; }

        /// <summary>
        /// Only if the filter expression is not sufficient, you can override this method to apply the filtering to the queryable directly.
        /// </summary>
        IQueryable<TAggregateRoot> Filter(IQueryable<TAggregateRoot> queryable);

        /// <summary>
        /// Implement a guard that might disallow adding to the repository.
        /// This overload is called directly before adding an instance, so that you can use the instance's state for deciding.
        /// </summary>
        bool CanCreate(TAggregateRoot t);

        /// <summary>
        /// Implement a guard that might disallow modifying an existing aggregate.
        /// This overload is called directly before saving modification of an instance, so that you can use the instance's state for deciding.
        /// </summary>
        bool CanModify(TAggregateRoot t);

        /// <summary>
        /// Implement a guard that might disallow deleting an existing aggregate.
        /// This overload is called directly before saving modification of an instance, so that you can use the instance's state for deciding.
        /// </summary>
        bool CanDelete(TAggregateRoot t);
    }
}
