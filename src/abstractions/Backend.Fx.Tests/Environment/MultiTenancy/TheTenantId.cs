using System;
using Backend.Fx.Environment.MultiTenancy;
using Xunit;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheTenantId
    {
        [Fact]
        public void HasNoValueWhenInitializedWithNull()
        {
            var sut = new TenantId(null);
            Assert.False(sut.HasValue);
        }

        [Fact]
        public void ThrowsOnAccessingTheValueWhenInitializedWithNull()
        {
            var sut = new TenantId(null);
            Assert.Throws<InvalidOperationException>(() => sut.Value);
        }
    }
}
