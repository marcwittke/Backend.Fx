using Backend.Fx.NLogLogging;
using Backend.Fx.RabbitMq.Tests;
using MarcWittke.Xunit.AssemblyFixture;
using Xunit;

[assembly:
    TestFramework(
        "MarcWittke.Xunit.AssemblyFixture.XunitTestFrameworkWithAssemblyFixture",
        "MarcWittke.Xunit.AssemblyFixture")]
[assembly: AssemblyFixture(typeof(TestLoggingFixture))]

namespace Backend.Fx.RabbitMq.Tests
{
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Backend.Fx")
        { }
    }
}
