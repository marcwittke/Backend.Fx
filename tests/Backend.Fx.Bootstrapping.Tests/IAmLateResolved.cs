namespace Backend.Fx.Bootstrapping.Tests
{
    using BuildingBlocks;
    public interface IAmLateResolved : IDomainService
    {

    }

    public class LateResolvedService : IAmLateResolved
    {
    }
}
