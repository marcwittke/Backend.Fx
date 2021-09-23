using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain
{
    public interface ITestApplicationService : IApplicationService
    { }


    public class AnApplicationService : ITestApplicationService
    { }
}
