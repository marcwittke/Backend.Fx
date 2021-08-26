using System.IO;
using System.Security.Principal;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.EfCorePersistence.Tests.Fixtures.Sqlite;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.NetCore.Logging;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    public class SqliteEfCoreSingletonServices : EfCoreSingletonServices<TestDbContext, TestScopedServices>
    {
        public SqliteEfCoreSingletonServices() : this("Data Source=" + Path.GetTempFileName())
        {
        }

        public SqliteEfCoreSingletonServices(string connectionString) : base(
            connectionString,
            new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connectionString).UseLoggerFactory(new FrameworkToBackendFxLoggerFactory()).Options,
            new EntityIdGenerator(),
            typeof(Blog).Assembly)
        {
        }

        public override TestScopedServices BeginScope(TenantId tenantId, IIdentity identity = null)
        {
            return new TestScopedServices(new TestDbContext(DbContextOptions), Clock, identity, tenantId, Assemblies);
        }
    }
}