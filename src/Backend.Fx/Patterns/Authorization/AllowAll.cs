namespace Backend.Fx.Patterns.Authorization
{
    using BuildingBlocks;

    public class AllowAll<TAggregateRoot> : AggregateAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {}
}