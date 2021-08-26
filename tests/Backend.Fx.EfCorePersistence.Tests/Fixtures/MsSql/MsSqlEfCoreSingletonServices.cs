using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.EfCorePersistence.Tests.Fixtures.MsSql;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.NetCore.Logging;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    public class MsSqlEfCoreSingletonServices : EfCoreSingletonServices<TestDbContext, TestScopedServices>
    {
        public MsSqlEfCoreSingletonServices(string connectionString) : base(
            connectionString,
            new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(connectionString).UseLoggerFactory(new FrameworkToBackendFxLoggerFactory()).Options,
            new EntityIdGenerator(new MsSqlConnectionFactory(connectionString)),
            typeof(Blog).Assembly)
        {
        }

        public override TestScopedServices BeginScope(TenantId tenantId, IIdentity identity = null)
        {
            return new TestScopedServices(new TestDbContext(DbContextOptions), Clock, identity, tenantId, Assemblies);
        }
        
        private class MsSqlConnectionFactory : IDbConnectionFactory
        {
            private readonly string _connectionString;

            public MsSqlConnectionFactory(string connectionString)
            {
                _connectionString = connectionString;
            }

            public IDbConnection Create()
            {
                return new SqlConnection(_connectionString);
            }
        }
    }
}