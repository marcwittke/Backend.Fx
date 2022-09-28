using System;
using System.Linq.Expressions;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Authorization
{
    public class AllowAll<TAggregateRoot, TId> : AuthorizationPolicy<TAggregateRoot, TId> 
        where TAggregateRoot : IAggregateRoot<TId> 
        where TId : struct, IEquatable<TId>
    {
        public override Expression<Func<TAggregateRoot, bool>> HasAccessExpression
        {
            get { return agg => true; }
        }

        public override bool CanCreate(TAggregateRoot t)
        {
            return true;
        }
    }
}