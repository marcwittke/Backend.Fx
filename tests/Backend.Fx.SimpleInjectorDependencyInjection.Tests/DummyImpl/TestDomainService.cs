using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl
{
    public interface ITestDomainService : IDomainService
    { }

    public interface IAnotherTestDomainService : IDomainService
    { }

    public class TestDomainService : ITestDomainService, IAnotherTestDomainService
    { }
}
