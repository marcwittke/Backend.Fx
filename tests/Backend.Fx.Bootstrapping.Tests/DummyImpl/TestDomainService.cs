namespace Backend.Fx.Bootstrapping.Tests.DummyImpl
{
    using BuildingBlocks;

    public interface ITestDomainService : IDomainService
    { }

    public interface IAnotherTestDomainService : IDomainService
    { }

    public class TestDomainService : ITestDomainService, IAnotherTestDomainService
    { }
}
