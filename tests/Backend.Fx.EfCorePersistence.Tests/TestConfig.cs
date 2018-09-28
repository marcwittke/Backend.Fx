using Backend.Fx.NLogLogging;

[assembly: Xunit.TestFramework("Backend.Fx.Testing.AssemblyFixtures.XunitTestFrameworkWithAssemblyFixture", "Backend.Fx.Testing")]
[assembly: Backend.Fx.Testing.AssemblyFixtures.AssemblyFixture(typeof(Backend.Fx.EfCorePersistence.Tests.TestLoggingFixture))]

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Backend.Fx")
        { }
    }
}