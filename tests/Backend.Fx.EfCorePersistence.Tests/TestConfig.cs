using Backend.Fx.NLogLogging;
using MarcWittke.Xunit.AssemblyFixture;

[assembly: Xunit.TestFramework("MarcWittke.Xunit.AssemblyFixture.XunitTestFrameworkWithAssemblyFixture", "MarcWittke.Xunit.AssemblyFixture")]
[assembly: AssemblyFixture(typeof(Backend.Fx.EfCorePersistence.Tests.TestLoggingFixture))]

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TestLoggingFixture : LoggingFixture
    {
        public TestLoggingFixture() : base("Backend.Fx")
        { }
    }
}