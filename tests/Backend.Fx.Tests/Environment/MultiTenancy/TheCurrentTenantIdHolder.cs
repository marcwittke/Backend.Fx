using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheCurrentTenantIdHolder : TestWithLogging
    {
        [Fact]
        public void InitializesWithNullTenantIdIdentity()
        {
            var currentTenantIdHolder = new CurrentTenantIdHolder();
            Assert.False(currentTenantIdHolder.Current.HasValue);
        }

        [Fact]
        public void ReplacesCurrentTenantId()
        {
            var currentTenantIdHolder = new CurrentTenantIdHolder();
            currentTenantIdHolder.ReplaceCurrent(new TenantId(345));
            Assert.Equal(345, currentTenantIdHolder.Current.Value);
        }

        public TheCurrentTenantIdHolder(ITestOutputHelper output) : base(output)
        {
        }
    }
}