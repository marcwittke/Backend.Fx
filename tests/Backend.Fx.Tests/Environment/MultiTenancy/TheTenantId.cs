namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    using System;
    using Fx.Environment.MultiTenancy;
    using NLogLogging;
    using Xunit;

    public class TheTenantId : IClassFixture<NLogLoggingFixture>
    {
        [Fact]
        public void HasNoValueWhenInitializedWithNull()
        {
            var sut = new TenantId(null);
            Assert.False(sut.HasValue);
        }

        public void ThrowsOnAccessingTHeValueWhenInitializedWithNull()
        {
            var sut = new TenantId(null);
            Assert.Throws<InvalidOperationException>(()=>sut.Value);
        }
    }
}
