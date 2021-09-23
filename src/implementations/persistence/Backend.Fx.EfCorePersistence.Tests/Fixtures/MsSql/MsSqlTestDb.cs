using Microsoft.Data.SqlClient;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures.MsSql
{
    public class MsSqlTestDb
    {
        public MsSqlTestDb(string testDbName)
        {
            var sqlUtil = MsSqlServerUtil.DetectSqlServer("BACKENDFXTESTDB");
            sqlUtil.EnsureDroppedDatabase(testDbName);
            sqlUtil.CreateDatabase(testDbName);

            var builder = new SqlConnectionStringBuilder(sqlUtil.ConnectionString)
            {
                InitialCatalog = testDbName,
                MultipleActiveResultSets = true
            };
            ConnectionString = builder.ConnectionString;
        }

        public string ConnectionString { get; }
    }
}
