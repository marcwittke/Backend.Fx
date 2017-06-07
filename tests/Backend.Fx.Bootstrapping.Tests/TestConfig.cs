[assembly: Xunit.TestFramework("Backend.Fx.Testing.AssemblyFixtures.XunitTestFrameworkWithAssemblyFixture", "Backend.Fx.Testing")]
[assembly: Backend.Fx.Testing.AssemblyFixtures.AssemblyFixture(typeof(Backend.Fx.Bootstrapping.Tests.TestLoggingFixture))]

namespace Backend.Fx.Bootstrapping.Tests
{
    using Testing.Logging;
   public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Backend.Fx")
        { }
    }
}