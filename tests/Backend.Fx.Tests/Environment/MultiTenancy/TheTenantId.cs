using System;
using Backend.Fx.Environment.MultiTenancy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheTenantId : TestWithLogging
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

        public TheTenantId(ITestOutputHelper output) : base(output)
        {
        }
    }
}