using Backend.Fx.NLogLogging;
using MarcWittke.Xunit.AssemblyFixture;

[assembly: Xunit.TestFramework("MarcWittke.Xunit.AssemblyFixture.XunitTestFrameworkWithAssemblyFixture", "MarcWittke.Xunit.AssemblyFixture")]
[assembly: AssemblyFixture(typeof(Backend.Fx.Tests.TestLoggingFixture))]

namespace Backend.Fx.Tests
{
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Not.Important.Since.Backend.Fx.Is.Logging.By.Default")
        { }
    }
}