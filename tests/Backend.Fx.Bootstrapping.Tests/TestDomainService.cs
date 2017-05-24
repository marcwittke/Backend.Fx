namespace Backend.Fx.Bootstrapping.Tests
{
    using BuildingBlocks;

    public interface ITestDomainService : IDomainService
    { }

    public class TestDomainService : ITestDomainService
    { }
}
