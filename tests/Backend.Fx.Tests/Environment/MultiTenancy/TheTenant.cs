using Xunit;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    using System;
    using System.Globalization;
    using Fx.Environment.MultiTenancy;

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
            // ReSharper disable once AssignNullToNotNullAttribute - testing null case exception
            Assert.Throws<ArgumentException>(() => new Tenant(null, "", false, CultureInfo.CurrentCulture));
            Assert.Throws<ArgumentException>(() => new Tenant("   ", "", false, CultureInfo.CurrentCulture));
        }
    }
}

