[assembly: Xunit.TestFramework("Backend.Fx.Testing.AssemblyFixtures.XunitTestFrameworkWithAssemblyFixture", "Backend.Fx.Testing")]
[assembly: Backend.Fx.Testing.AssemblyFixtures.AssemblyFixture(typeof(Backend.Fx.Tests.TestLoggingFixture))]

namespace Backend.Fx.Tests
{
    using Testing.Logging;
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Not.Important.Since.Backend.Fx.Is.Logging.By.Default")
        { }
    }
}