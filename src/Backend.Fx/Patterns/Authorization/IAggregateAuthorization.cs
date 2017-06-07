namespace Backend.Fx.Patterns.Authorization
{
    using System.Linq.Expressions;
    using BuildingBlocks;

    public interface IAggregateAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        /// <summary>
        /// Express a filter for repository queryable
        /// </summary>
        Expression<System.Func<TAggregateRoot, bool>> HasAccessExpression { get; }

        /// <summary>
        /// Implement a guard that might disallow adding to the repository
        /// </summary>
        bool CanCreate();

        /// <summary>
        /// Implement a guard that might disallow adding to the repository.
        /// This overload is called directly before adding an instance, so that you can use the instance's state for deciding.
        /// Both overloads must return <code>true</code> to be considered as permission alloewance.
        /// </summary>
        bool CanCreate(TAggregateRoot t);
    }
}
