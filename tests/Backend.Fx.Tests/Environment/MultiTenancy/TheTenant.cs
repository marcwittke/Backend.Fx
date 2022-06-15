using System;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheTenant : TestWithLogging
    {
        [Fact]
        public void CannotBeInitializedWithoutName()
        {
            Assert.Throws<ArgumentException>(() => new Tenant("", "", false));
            // ReSharper disable once AssignNullToNotNullAttribute - testing null case exception
            Assert.Throws<ArgumentException>(() => new Tenant(null, "", false));
            Assert.Throws<ArgumentException>(() => new Tenant("   ", "", false));
        }

        [Fact]
        public void InitializesCorrectly()
        {
            var tenant = new Tenant("name", "description", true);
            Assert.Equal("name", tenant.Name);
            Assert.Equal("description", tenant.Description);
            Assert.True(tenant.IsDemoTenant);
        }

        public TheTenant(ITestOutputHelper output) : base(output)
        {
        }
    }
}