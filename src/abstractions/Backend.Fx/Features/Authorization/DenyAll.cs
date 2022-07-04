using System;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.Features.Authorization
{
    public class DenyAll<TAggregateRoot> : AggregateAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        public override Expression<Func<TAggregateRoot, bool>> HasAccessExpression
        {
            get { return agg => false; }
        }

        public override bool CanCreate(TAggregateRoot t)
        {
            return false;
        }
    }
}