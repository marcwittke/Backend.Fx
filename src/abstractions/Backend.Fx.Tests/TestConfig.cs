using Backend.Fx.NLogLogging;
using Backend.Fx.Tests;
using MarcWittke.Xunit.AssemblyFixture;
using Xunit;

[assembly:
    TestFramework(
        "MarcWittke.Xunit.AssemblyFixture.XunitTestFrameworkWithAssemblyFixture",
        "MarcWittke.Xunit.AssemblyFixture")]
[assembly: AssemblyFixture(typeof(TestLoggingFixture))]

namespace Backend.Fx.Tests
{
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Not.Important.Since.Backend.Fx.Is.Logging.By.Default")
        { }
    }
}
