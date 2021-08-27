using Backend.Fx.Environment.Authentication;
using Xunit;

namespace Backend.Fx.Tests.Environment.Authentication
{
    public class TheSystemIdentity
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
    }
}