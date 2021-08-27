using Backend.Fx.Environment.Authentication;
using Xunit;

namespace Backend.Fx.Tests.Environment.Authentication
{
    public class TheAnonymousIdentity
    {
        [Fact]
        public void HasNameAnonymous()
        {
            Assert.Equal("ANONYMOUS", new AnonymousIdentity().Name);
        }

        [Fact]
        public void HasNoAuthenticationType()
        {
            Assert.Equal(string.Empty, new AnonymousIdentity().AuthenticationType);
        }

        [Fact]
        public void IsNotAuthenticated()
        {
            Assert.False(new AnonymousIdentity().IsAuthenticated);
        }
    }
}