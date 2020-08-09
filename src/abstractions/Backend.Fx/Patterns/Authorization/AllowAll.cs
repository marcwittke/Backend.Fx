using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.Patterns.Authorization
{
    public class AllowAll<TAggregateRoot> : AggregateAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
    }
}