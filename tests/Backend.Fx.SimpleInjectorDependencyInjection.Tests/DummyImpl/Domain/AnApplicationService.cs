using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain
{
    public interface ITestApplicationService : IApplicationService
    { }

    public class AnApplicationService : ITestApplicationService
    { }
}
