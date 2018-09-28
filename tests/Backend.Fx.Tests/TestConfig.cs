using Backend.Fx.NLogLogging;
using Backend.Fx.Xunit.AssemblyFixtures;

[assembly: Xunit.TestFramework("Backend.Fx.Testing.AssemblyFixtures.XunitTestFrameworkWithAssemblyFixture", "Backend.Fx.Testing")]
[assembly: AssemblyFixture(typeof(Backend.Fx.Tests.TestLoggingFixture))]

namespace Backend.Fx.Tests
{
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Not.Important.Since.Backend.Fx.Is.Logging.By.Default")
        { }
    }
}