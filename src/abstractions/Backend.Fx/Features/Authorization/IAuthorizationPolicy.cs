using System;
using System.Linq.Expressions;
using Backend.Fx.Domain;
using JetBrains.Annotations;

namespace Backend.Fx.Features.Authorization
{
    /// <summary>
    /// Implements permissions on aggregate level. The respective instance is applied when creating an <see cref="IRepository{T}"/>,
    /// so that the repository never allows reading or writing of an aggregate without permissions. 
    /// </summary>
    [PublicAPI]
    public interface IAuthorizationPolicy<TAggregateRoot> 
        where TAggregateRoot : IAggregateRoot
    {
        /// <summary>
        /// Express a filter for repository queryable
        /// </summary>
        Expression<Func<TAggregateRoot, bool>> HasAccessExpression { get; }

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