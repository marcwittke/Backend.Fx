namespace Backend.Fx.Tests.Environment.Authentication
{
    using Fx.Environment.Authentication;
    using Xunit;

    public class TheAnonymousIdentity
    {
        [Fact]
        public void IsNotAuthenticated()
        {
            Assert.False(new AnonymousIdentity().IsAuthenticated);
        }

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
    }
}