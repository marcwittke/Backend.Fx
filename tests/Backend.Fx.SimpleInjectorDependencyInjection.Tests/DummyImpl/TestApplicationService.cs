using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl
{
    public interface ITestApplicationService : IApplicationService
    { }

    public class TestApplicationService : ITestApplicationService
    { }
}
