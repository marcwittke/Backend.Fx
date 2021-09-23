using Backend.Fx.Environment.Authentication;
using Xunit;

namespace Backend.Fx.Tests.Environment.Authentication
{
    public class TheCurrentIdentityHolder
    {
        [Fact]
        public void FallsBackToInitialValueWhenReplacingWithNull()
        {
            var currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(null);
            Assert.Equal("ANONYMOUS", currentIdentityHolder.Current.Name);
        }

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
    }
}
