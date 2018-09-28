namespace Backend.Fx.Patterns.Authorization
{
    using System;
    using System.Linq.Expressions;
    using BuildingBlocks;

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

        public override bool CanModify(TAggregateRoot t)
        {
            return false;
        }
    }
}