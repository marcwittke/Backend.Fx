using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    [Obsolete("Not supported on build agents")]
    public class SqlServerDatabaseFixture : DatabaseFixture
    {
        private static int _testindex = 1;
        private readonly string _connectionString;

        public SqlServerDatabaseFixture()
        {
            string dbName = $"TestFixture_{_testindex++:000}";
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder("Server=.\\SQLExpress;Trusted_Connection=True;");
            using (IDbConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var dropCommand = connection.CreateCommand())
                {
                    dropCommand.CommandText = $"IF EXISTS(SELECT * FROM sys.Databases WHERE Name='{dbName}') ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    dropCommand.ExecuteNonQuery();
                }

                using (var dropCommand = connection.CreateCommand())
                {
                    dropCommand.CommandText = $"IF EXISTS(SELECT * FROM sys.Databases WHERE Name='{dbName}') DROP DATABASE [{dbName}]";
                    dropCommand.ExecuteNonQuery();
                }

                using (var createCommand = connection.CreateCommand())
                {
                    createCommand.CommandText = $"CREATE DATABASE [{dbName}]";
                    createCommand.ExecuteNonQuery();
                }
                connection.Close();
            }

            sqlConnectionStringBuilder.InitialCatalog = dbName;
            _connectionString = sqlConnectionStringBuilder.ConnectionString;
        }

        protected override DbContextOptions<TestDbContext> GetDbContextOptionsForDbCreation()
        {
            return new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(_connectionString).Options;
        }

        protected override DbContextOptions<TestDbContext> GetDbContextOptions(DbConnection connection)
        {
            return new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(connection).Options;
        }

        public override DbSession UseDbSession()
        {
            var sqlConnection = new SqlConnection(_connectionString);
            return new DbSession(sqlConnection, GetDbContextOptions(sqlConnection));
        }
    }
}