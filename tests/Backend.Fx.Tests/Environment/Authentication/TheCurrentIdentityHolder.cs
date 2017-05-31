namespace Backend.Fx.Tests.Environment.Authentication
{
    using Fx.Environment.Authentication;
    using NLogLogging;
    using Xunit;

    public class TheCurrentIdentityHolder : IClassFixture<NLogLoggingFixture>
    {
        [Fact]
        public void InitializesWithAnonymousIdentity()
        {
            var currentIdentityHolder = new CurrentIdentityHolder();
            Assert.Equal("ANONYMOUS", currentIdentityHolder.Current.Name);
        }

        [Fact]
        public void ReplacesCurrentIdentity()
        {
            var currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(new SystemIdentity());
            Assert.Equal("SYSTEM", currentIdentityHolder.Current.Name);
        }

        [Fact]
        public void FallsBackToInitialValueWhenReplacingWithNull()
        {
            var currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(null);
            Assert.Equal("ANONYMOUS", currentIdentityHolder.Current.Name);
        }
    }
}
