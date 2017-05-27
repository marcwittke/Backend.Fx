namespace Backend.Fx.Bootstrapping.Tests.DummyImpl
{
    using BuildingBlocks;

    public interface IAmLateResolved : IDomainService
    {

    }

    public class LateResolvedService : IAmLateResolved
    {
    }
}
