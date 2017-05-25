namespace Backend.Fx.Patterns.Authorization
{
    using System;
    using System.Linq.Expressions;
    using BuildingBlocks;

    public interface IAggregateRootAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        System.Linq.Expressions.Expression<System.Func<TAggregateRoot, bool>> HasAccessExpression { get; }

        bool CanCreate();
    }
}
