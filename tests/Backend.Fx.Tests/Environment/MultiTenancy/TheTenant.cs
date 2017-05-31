namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    using System;
    using Fx.Environment.MultiTenancy;
    using NLogLogging;
    using Xunit;

    public class TheTenant : IClassFixture<NLogLoggingFixture>
    {
        [Fact]
        public void InitializesCorrectly()
        {
            Tenant tenant = new Tenant("name", "description", true);
            Assert.Equal("name", tenant.Name);
            Assert.Equal("description", tenant.Description);
            Assert.True(tenant.IsDemoTenant);
        }

        [Fact]
        public void CannotBeInitializedWithoutName()
        {
            Assert.Throws<ArgumentException>(() => new Tenant("", "", false));
            Assert.Throws<ArgumentException>(() => new Tenant(null, "", false));
            Assert.Throws<ArgumentException>(() => new Tenant("   ", "", false));
        }
    }
}

