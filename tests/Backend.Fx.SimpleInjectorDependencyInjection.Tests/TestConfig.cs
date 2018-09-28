using Backend.Fx.NLogLogging;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests;

[assembly: Xunit.TestFramework("Backend.Fx.Testing.AssemblyFixtures.XunitTestFrameworkWithAssemblyFixture", "Backend.Fx.Testing")]
[assembly: Backend.Fx.Testing.AssemblyFixtures.AssemblyFixture(typeof(TestLoggingFixture))]

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Backend.Fx")
        { }
    }
}