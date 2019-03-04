namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    using System;
    using Fx.Environment.MultiTenancy;
    using Xunit;

    public class TheTenant
    {
        [Fact]
        public void InitializesCorrectly()
        {
            Tenant tenant = new Tenant("name", "description", true, "de-DE");
            Assert.Equal("name", tenant.Name);
            Assert.Equal("description", tenant.Description);
            Assert.True(tenant.IsDemoTenant);
        }

        [Fact]
        public void CannotBeInitializedWithoutName()
        {
            Assert.Throws<ArgumentException>(() => new Tenant("", "", false, "en-US"));
            // ReSharper disable once AssignNullToNotNullAttribute - testing null case exception
            Assert.Throws<ArgumentException>(() => new Tenant(null, "", false, "es-AR"));
            Assert.Throws<ArgumentException>(() => new Tenant("   ", "", false, "fr-FR"));
        }
    }
}

