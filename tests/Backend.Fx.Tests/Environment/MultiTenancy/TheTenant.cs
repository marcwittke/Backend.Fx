namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    using System;
    using System.Globalization;
    using Fx.Environment.MultiTenancy;
    using Xunit;

    public class TheTenant
    {
        [Fact]
        public void InitializesCorrectly()
        {
            Tenant tenant = new Tenant("name", "description", true, CultureInfo.CurrentCulture);
            Assert.Equal("name", tenant.Name);
            Assert.Equal("description", tenant.Description);
            Assert.True(tenant.IsDemoTenant);
        }

        [Fact]
        public void CannotBeInitializedWithoutName()
        {
            Assert.Throws<ArgumentException>(() => new Tenant("", "", false, CultureInfo.CurrentCulture));
            Assert.Throws<ArgumentException>(() => new Tenant(null, "", false, CultureInfo.CurrentCulture));
            Assert.Throws<ArgumentException>(() => new Tenant("   ", "", false, CultureInfo.CurrentCulture));
        }
    }
}

