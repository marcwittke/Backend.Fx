namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    using System;
    using Fx.Environment.MultiTenancy;
    using Xunit;

    public class TheTenantId
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
