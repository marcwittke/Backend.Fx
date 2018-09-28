using Backend.Fx.NLogLogging;
using Backend.Fx.Xunit.AssemblyFixtures;

[assembly: Xunit.TestFramework("Backend.Fx.Testing.AssemblyFixtures.XunitTestFrameworkWithAssemblyFixture", "Backend.Fx.Testing")]
[assembly: AssemblyFixture(typeof(Backend.Fx.EfCorePersistence.Tests.TestLoggingFixture))]

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Backend.Fx")
        { }
    }
}