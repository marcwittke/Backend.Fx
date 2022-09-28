using System;
using System.Linq.Expressions;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Persistence
{
    public interface ITenantFilterExpressionFactory<TAggregateRoot, TId> where TAggregateRoot : IAggregateRoot<TId> 
        where TId : struct, IEquatable<TId>
    {
        Expression<TAggregateRoot> CreateTenantFilterExpression();
    }
}