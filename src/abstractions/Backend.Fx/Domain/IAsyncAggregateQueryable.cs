using System.Linq;

namespace Backend.Fx.Domain
{
    public interface IAsyncAggregateQueryable<out TAggregateRoot> : IAsyncQueryable<TAggregateRoot>
        where TAggregateRoot: AggregateRoot
    { }
}