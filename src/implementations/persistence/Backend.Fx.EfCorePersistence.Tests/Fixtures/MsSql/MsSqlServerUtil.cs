using System;
using Backend.Fx.Logging;
using Microsoft.Data.SqlClient;
using Polly;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures.MsSql
{
    public class MsSqlServerUtil
    {
        private static readonly ILogger Logger = LogManager.Create<MsSqlServerUtil>();

        public MsSqlServerUtil(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        public static MsSqlServerUtil DetectSqlServer(string connectionStringEnvironmentVariable = null)
        {
            // the build agent is in charge to start a sql server (e.g. using docker) and to provide the connection string 
            // via environment variable. If the environment variable is set, we're doing some retries allowing the sql server 
            // to get up and running

            // other generally known local sql instances are either up or considered as non existent
            string environmentVariableValue = connectionStringEnvironmentVariable == null
                ? null
                : System.Environment.GetEnvironmentVariable(connectionStringEnvironmentVariable);

            string connectionString = DetectLocalSqlServerWithRetries(environmentVariableValue, 7)
                ?? DetectLocalSqlServer("Server=.\\SQLExpress;Integrated Security=true;")
                ?? DetectLocalSqlServer("Server=.;Integrated Security=true;")
                ?? DetectLocalSqlServer(
                    "Server=localhost;User=sa;Password=yourStrong(!)Password"); // default from docker hub

            if (connectionString != null)
            {
                Logger.Info("Detected MSSQL Connection string for test execution: " + connectionString);
                return new MsSqlServerUtil(connectionString);
            }

            throw new InvalidOperationException("Cannot run tests because no SQL database was detected");
        }

        private static string DetectLocalSqlServerWithRetries(string connectionString, int retries)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                Logger.Info(
                    $"Probing for SQL instance using connection string {connectionString} with {retries} retries.");
                return Policy
                    .HandleResult<string>(r => r == null)
                    .WaitAndRetry(
                        retries,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(retryAttempt, 2)) /* 1,2,4,8,16,32,64,128 secs */)
                    .Execute(() => DetectLocalSqlServer(connectionString));
            }

            return null;
        }

        private static string DetectLocalSqlServer(string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "SELECT 1";
                        command.ExecuteScalar();
                    }

                    return connectionString;
                }
                catch (Exception ex)
                {
                    Logger.Info(ex, $"No MSSQL instance was found using connection string [{connectionString}]");
                }
            }

            return null;
        }

        public void EnsureExistingDatabase()
        {
            var sb = new SqlConnectionStringBuilder(ConnectionString);
            string dbName = sb.InitialCatalog;
            sb.InitialCatalog = "master";
            using (var connection = new SqlConnection(sb.ConnectionString))
            {
                connection.Open();
                bool isExistent;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT count(*) FROM sys.databases WHERE Name = '{dbName}'";
                    isExistent = (int)command.ExecuteScalar() == 1;
                }

                if (!isExistent)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "CREATE DATABASE [" + dbName + "]";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public void EnsureDroppedDatabase(string dbName)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var dropCommand = connection.CreateCommand())
                {
                    dropCommand.CommandText =
                        $"IF EXISTS(SELECT * FROM sys.Databases WHERE Name='{dbName}') ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    dropCommand.ExecuteNonQuery();
                }

                using (var dropCommand = connection.CreateCommand())
                {
                    dropCommand.CommandText
                        = $"IF EXISTS(SELECT * FROM sys.Databases WHERE Name='{dbName}') DROP DATABASE [{dbName}]";
                    dropCommand.ExecuteNonQuery();
                }
            }
        }

        public void CreateDatabase(string dbName)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var createCommand = connection.CreateCommand())
                {
                    createCommand.CommandText = $"CREATE DATABASE [{dbName}]";
                    createCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
