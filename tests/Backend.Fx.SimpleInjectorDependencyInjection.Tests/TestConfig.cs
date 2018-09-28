using Backend.Fx.SimpleInjectorDependencyInjection.Tests;
using Backend.Fx.Testing.Logging;

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