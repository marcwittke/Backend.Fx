using System;
using System.Linq.Expressions;
using Backend.Fx.Domain;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Authorization
{
    public abstract class AuthorizationPolicy<TAggregateRoot> : IAuthorizationPolicy<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = Log.Create<AuthorizationPolicy<TAggregateRoot>>();
        
        /// <inheritdoc />>
        public abstract Expression<Func<TAggregateRoot, bool>> HasAccessExpression { get; }

        /// <inheritdoc />>
        public abstract bool CanCreate(TAggregateRoot t);

        /// <summary>
        /// Implement a guard that might disallow modifying an existing aggregate.
        /// This overload is called directly before saving modification of an instance, so that you can use the instance's state for deciding.
        /// This default implementation forwards to <see cref="AuthorizationPolicy{TAggregateRoot}.CanCreate"/>
        /// </summary>
        public virtual bool CanModify(TAggregateRoot t)
        {
            var canCreate = CanCreate(t);
            Logger.LogTrace("CanCreate({AggregateRootTypeName}): {CanCreate}", t.DebuggerDisplay, canCreate);
            return canCreate;
        }

        /// <inheritdoc />>
        public virtual bool CanDelete(TAggregateRoot t)
        {
            var canModify = CanModify(t);
            Logger.LogTrace("CanModify({AggregateRootTypeName}): {CanCreate}", t.DebuggerDisplay, canModify);
            return canModify;
        }
    }
}