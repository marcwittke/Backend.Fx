[assembly: Xunit.TestFramework("Backend.Fx.Testing.AssemblyFixtures.XunitTestFrameworkWithAssemblyFixture", "Backend.Fx.Testing")]
[assembly: Backend.Fx.Testing.AssemblyFixtures.AssemblyFixture(typeof(Backend.Fx.Tests.TestLoggingFixture))]

namespace Backend.Fx.Tests
{
    using Testing.Logging;
   public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Backend.Fx")
        { }
    }
}