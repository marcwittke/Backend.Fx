using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests
{
    [Obsolete("Not supported on build agents")]
    public class TestWithSqlServerDbContext : TestWithDbContext
    {
        public override DbContextOptions<TestDbContext> DbContextOptions()
        {
            return new DbContextOptionsBuilder<TestDbContext>().UseSqlServer((DbConnection)Connection).Options;
        }

        public override TestDbContext UseDbContext()
        {
            DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>().UseSqlServer((DbConnection)Connection).Options;
            return new TestDbContext(options);
        }

        public TestWithSqlServerDbContext(string dbName, string connectionString = "Server=.\\SQLExpress;Trusted_Connection=True") 
            : base(BuildConnection(connectionString, dbName))
        {
            ConnectionString = new SqlConnectionStringBuilder(connectionString) {InitialCatalog = dbName}.ConnectionString;
        }

        public string ConnectionString { get; }

        private static IDbConnection BuildConnection(string connectionString, string dbName)
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            sqlConnectionStringBuilder.InitialCatalog = "master";
            using (SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString))
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
            }

            sqlConnectionStringBuilder.InitialCatalog = dbName;
            var sqlConnection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
            sqlConnection.Open();
            return sqlConnection;
        }
    }
}