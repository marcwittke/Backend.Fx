using System;
using System.Data;
using System.Data.SqlClient;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
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
            var dbName = $"TestFixture_{_testindex++:000}";
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder("Server=.\\SQLExpress;Trusted_Connection=True;");
            using (IDbConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (IDbCommand dropCommand = connection.CreateCommand())
                {
                    dropCommand.CommandText = $"IF EXISTS(SELECT * FROM sys.Databases WHERE Name='{dbName}') ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    dropCommand.ExecuteNonQuery();
                }

                using (IDbCommand dropCommand = connection.CreateCommand())
                {
                    dropCommand.CommandText = $"IF EXISTS(SELECT * FROM sys.Databases WHERE Name='{dbName}') DROP DATABASE [{dbName}]";
                    dropCommand.ExecuteNonQuery();
                }

                using (IDbCommand createCommand = connection.CreateCommand())
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


        public override DbContextOptionsBuilder<TestDbContext> GetDbContextOptionsBuilder(IDbConnection connection)
        {
            return new DbContextOptionsBuilder<TestDbContext>().UseSqlServer((SqlConnection) connection);
        }

        public override DbConnectionOperationDecorator UseOperation()
        {
            var sqliteConnection = new SqlConnection(_connectionString);
            IOperation operation = new Operation();
            operation = new DbTransactionOperationDecorator(sqliteConnection, operation);
            return new DbConnectionOperationDecorator(sqliteConnection, operation);
        }
    }
}