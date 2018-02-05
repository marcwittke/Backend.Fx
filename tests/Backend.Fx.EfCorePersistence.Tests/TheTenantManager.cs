namespace Backend.Fx.EfCorePersistence.Tests
{
    using System.Globalization;
    using System.Linq;
    using DummyImpl;
    using Environment.MultiTenancy;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class TheTenantManager : TestWithInMemorySqliteDbContext
    {
        private readonly MyTenantManager sut;

        public TheTenantManager()
        {
            CreateDatabase();
            sut = new MyTenantManager(A.Fake<ITenantInitializer>(), DbContextOptions);
        }

        [Fact]
        public void CanCreateTenant()
        {
            var tenant = new Tenant("Tenant 1", "Lorem ipsum", false, CultureInfo.CurrentCulture);
            sut.SaveTenantX(tenant);

            Assert.Equal(1, ExecuteScalar<long>("SELECT Count(*) from Tenants"));
        }

        [Fact]
        public void CanUpdateTenant()
        {
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4711', 'The Tenant', 'The Description', 2, 0, 0)");

            var tenant = new Tenant("Tenant 1", "Lorem Ipsum", false, CultureInfo.CurrentCulture) { Id = 4711 };
            sut.SaveTenantX(tenant);

            Assert.Equal(1, ExecuteScalar<long>("SELECT Count(*) from Tenants"));
            Assert.Equal("Tenant 1", ExecuteScalar<string>("SELECT Name from Tenants WHERE Id='4711'"));
            Assert.Equal("Lorem Ipsum", ExecuteScalar<string>("SELECT Description from Tenants WHERE Id='4711'"));
        }

        [Fact]
        public void CanFindTenant()
        {
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4711', 'The Tenant', 'The Description', 2, 1, 0)");

            var tenant = sut.FindTenantX(new TenantId(4711));
            Assert.Equal("The Tenant", tenant.Name);
            Assert.Equal("The Description", tenant.Description);
        }

        [Fact]
        public void CanGetTenants()
        {
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4711', 'The Tenant 1', 'The Description 1', 2, 1, 0)");
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4712', 'The Tenant 2', 'The Description 2', 2, 1, 0)");
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4713', 'The Tenant 3', 'The Description 3', 2, 1, 0)");
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4714', 'The Tenant 4', 'The Description 4', 2, 1, 0)");

            var tenants = sut.GetTenants();
            Assert.Equal(4, tenants.Length);
            Assert.True(tenants.Any(t => t.Id == 4711));
            Assert.True(tenants.Any(t => t.Id == 4712));
            Assert.True(tenants.Any(t => t.Id == 4713));
            Assert.True(tenants.Any(t => t.Id == 4714));
        }

        [Fact]
        public void CanGetTenantIds()
        {
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4711', 'The Tenant 1', 'The Description 1', 2, 1, 0)");
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4712', 'The Tenant 2', 'The Description 2', 2, 1, 0)");
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4713', 'The Tenant 3', 'The Description 3', 2, 1, 0)");
            ExecuteNonQuery("INSERT INTO Tenants (Id, Name, Description, State, IsDemoTenant, IsDefault) VALUES ('4714', 'The Tenant 4', 'The Description 4', 2, 1, 0)");

            var tenants = sut.GetTenantIds();
            Assert.Equal(4, tenants.Length);
            Assert.True(tenants.Any(t => t.Value == 4711));
            Assert.True(tenants.Any(t => t.Value == 4712));
            Assert.True(tenants.Any(t => t.Value == 4713));
            Assert.True(tenants.Any(t => t.Value == 4714));
        }

        private class MyTenantManager : TenantManager<TestDbContext>
        {
            public MyTenantManager(ITenantInitializer tenantInitializer, DbContextOptions<TestDbContext> dbContextOptions) : base(tenantInitializer, dbContextOptions)
            { }

            public Tenant FindTenantX(TenantId tenantId)
            {
                return FindTenant(tenantId);
            }

            public void SaveTenantX(Tenant tenant)
            {
                SaveTenant(tenant);
            }
        }
    }
}
