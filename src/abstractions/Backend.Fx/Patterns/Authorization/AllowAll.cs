using System;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.Patterns.Authorization
{
    public class AllowAll<TAggregateRoot> : AggregateAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
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