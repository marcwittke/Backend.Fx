using System;
using System.Data;
using System.Data.SqlClient;
using Backend.Fx.EfCore6Persistence.Tests.SampleApp.Persistence;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Extensions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence.Tests.Fixtures
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

        protected override DbContextOptions<SampleAppDbContext> GetDbContextOptionsForDbCreation()
        {
            return new DbContextOptionsBuilder<SampleAppDbContext>().UseSqlServer(_connectionString).Options;
        }


        public override DbContextOptionsBuilder<SampleAppDbContext> GetDbContextOptionsBuilder(IDbConnection connection)
        {
            return new DbContextOptionsBuilder<SampleAppDbContext>().UseSqlServer((SqlConnection) connection);
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