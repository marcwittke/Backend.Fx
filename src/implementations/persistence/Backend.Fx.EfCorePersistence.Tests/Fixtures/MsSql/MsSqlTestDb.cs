using Microsoft.Data.SqlClient;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    public class MsSqlTestDb
    {
        public string ConnectionString { get; set; }

        public MsSqlTestDb(string testDbName)
        {
            var sqlUtil = MsSqlServerUtil.DetectSqlServer("BACKENDFXTESTDB");
            sqlUtil.EnsureDroppedDatabase(testDbName);
            sqlUtil.CreateDatabase(testDbName);

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(sqlUtil.ConnectionString)
            {
                InitialCatalog = testDbName,
                MultipleActiveResultSets = true,
            };
            ConnectionString = builder.ConnectionString;
        }
    }
}