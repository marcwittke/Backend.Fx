﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.Authorization
{
    public abstract class AggregateAuthorization<TAggregateRoot> : IAggregateAuthorization<TAggregateRoot>
        where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<AggregateAuthorization<TAggregateRoot>>();

        /// <inheritdoc />
        /// >
        public abstract Expression<Func<TAggregateRoot, bool>> HasAccessExpression { get; }

        /// <inheritdoc />
        /// >
        public virtual IQueryable<TAggregateRoot> Filter(IQueryable<TAggregateRoot> queryable)
        {
            return queryable.Where(HasAccessExpression);
        }

        /// <inheritdoc />
        /// >
        public abstract bool CanCreate(TAggregateRoot t);

        /// <summary>
        /// Implement a guard that might disallow modifying an existing aggregate.
        /// This overload is called directly before saving modification of an instance, so that you can use the instance's state
        /// for deciding.
        /// This default implementation forwards to <see cref="AggregateAuthorization{TAggregateRoot}.CanCreate" />
        /// </summary>
        public virtual bool CanModify(TAggregateRoot t)
        {
            bool canCreate = CanCreate(t);
            Logger.Trace($"CanCreate({t.DebuggerDisplay}): {canCreate}");
            return canCreate;
        }

        /// <inheritdoc />
        /// >
        public virtual bool CanDelete(TAggregateRoot t)
        {
            bool canModify = CanModify(t);
            Logger.Trace($"CanModify({t.DebuggerDisplay}): {canModify}");
            return canModify;
        }
    }
}
