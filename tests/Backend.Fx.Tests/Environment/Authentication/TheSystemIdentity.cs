using Backend.Fx.ExecutionPipeline;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.Authentication
{
    public class TheSystemIdentity : TestWithLogging
    {
        [Fact]
        public void HasAuthenticationTypeSystemInternal()
        {
            Assert.Equal("system internal", new SystemIdentity().AuthenticationType);
        }

        [Fact]
        public void HasNameSystem()
        {
            Assert.Equal("SYSTEM", new SystemIdentity().Name);
        }

        [Fact]
        public void IsAuthenticated()
        {
            Assert.True(new SystemIdentity().IsAuthenticated);
        }

        public TheSystemIdentity(ITestOutputHelper output) : base(output)
        {
        }
    }
}