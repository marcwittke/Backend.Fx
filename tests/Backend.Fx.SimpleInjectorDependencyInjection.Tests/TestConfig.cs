using Backend.Fx.NLogLogging;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests;
using MarcWittke.Xunit.AssemblyFixture;

[assembly: Xunit.TestFramework("MarcWittke.Xunit.AssemblyFixture.XunitTestFrameworkWithAssemblyFixture", "MarcWittke.Xunit.AssemblyFixture")]
[assembly: AssemblyFixture(typeof(TestLoggingFixture))]

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Backend.Fx")
        { }
    }
}