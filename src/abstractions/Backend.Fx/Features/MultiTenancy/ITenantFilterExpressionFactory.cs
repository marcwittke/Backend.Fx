using System.Linq.Expressions;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.MultiTenancy
{
    public interface ITenantFilterExpressionFactory<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        Expression<TAggregateRoot> CreateTenantFilterExpression();
    }
}