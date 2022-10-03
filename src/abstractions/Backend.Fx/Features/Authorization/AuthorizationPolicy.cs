using System;
using System.Linq.Expressions;
using Backend.Fx.Domain;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Authorization
{
    public abstract class AuthorizationPolicy<TAggregateRoot, TId> : IAuthorizationPolicy<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
        private static readonly ILogger Logger = Log.Create<AuthorizationPolicy<TAggregateRoot, TId>>();
        
        /// <inheritdoc />>
        public abstract Expression<Func<TAggregateRoot, bool>> HasAccessExpression { get; }

        /// <inheritdoc />>
        public abstract bool CanCreate(TAggregateRoot t);

        /// <summary>
        /// Implement a guard that might disallow modifying an existing aggregate.
        /// This overload is called directly before saving modification of an instance, so that you can use the instance's state for deciding.
        /// This default implementation forwards to <see cref="AuthorizationPolicy{TAggregateRoot, TId}.CanCreate"/>
        /// </summary>
        public virtual bool CanModify(TAggregateRoot t)
        {
            var canModify = CanCreate(t);
            Logger.LogTrace("CanModify({@Aggregate}): {CanModify}", t, canModify);
            return canModify;
        }

        /// <inheritdoc />>
        public virtual bool CanDelete(TAggregateRoot t)
        {
            var canDelete = CanModify(t);
            Logger.LogTrace("CanDelete({@Aggregate}): {CanDelete}", t, canDelete);
            return canDelete;
        }
    }
}