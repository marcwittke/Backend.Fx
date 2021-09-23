using System.Security.Principal;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.EfCorePersistence.Tests.Fixtures.MsSql;
using Backend.Fx.EfCorePersistence.Tests.Fixtures.Sqlite;
using Backend.Fx.Environment.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    public class PersistenceFixture
    {
        public static bool RunTestsWithSqlServerDatabase = false;

        public PersistenceFixture()
        {
            if (RunTestsWithSqlServerDatabase)
            {
                var testDb = new MsSqlTestDb("BackendFxEfCorePersistenceTests");
                SingletonServices = new MsSqlEfCoreSingletonServices(testDb.ConnectionString);
            }
            else
            {
                SingletonServices = new SqliteEfCoreSingletonServices();
            }

            using (var dbContext = UseDbContext())
            {
                dbContext.Database.EnsureCreated();
            }
        }

        public EfCoreSingletonServices<TestDbContext, TestScopedServices> SingletonServices { get; }

        public TenantId TenantId { get; } = new(999);

        public string ConnectionString => SingletonServices.ConnectionString;

        public TestScopedServices BeginScope(IIdentity identity = null)
        {
            return SingletonServices.BeginScope(TenantId, identity);
        }

        public DbContext UseDbContext()
        {
            return new TestDbContext(SingletonServices.DbContextOptions);
        }
    }
}
