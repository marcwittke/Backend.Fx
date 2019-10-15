using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Polly;

namespace Backend.Fx.SqlServer
{
    public class MsSqlServerUtil : IDatabaseUtil
    {
        private static readonly ILogger Logger = LogManager.Create<MsSqlServerUtil>();

        public MsSqlServerUtil(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        public static MsSqlServerUtil DetectSqlServer(string connectionStringEnvironmentVariable=null)
        {
            // the build agent is in charge to start a sql server (e.g. using docker) and to provide the connection string 
            // via environment variable. If the environment variable is set, we're doing some retries allowing the sql server 
            // to get up and running

            // other generally known local sql instances are either up or considered as non existant
            var environmentVariableValue = connectionStringEnvironmentVariable == null 
                ? null 
                : System.Environment.GetEnvironmentVariable(connectionStringEnvironmentVariable);

            string connectionString = DetectLocalSqlServerWithRetries(environmentVariableValue, 7)
                    ?? DetectLocalSqlServer("Server=.\\SQLExpress;Integrated Security=true;")
                    ?? DetectLocalSqlServer("Server=.;Integrated Security=true;");

            if (connectionString != null)
            {
                Logger.Info("Detected MSSQL Connection string for test execution: " + connectionString);
                return new MsSqlServerUtil(connectionString);
            }

            throw new InvalidOperationException("Cannot run tests because neither a local SQL database nor a Docker API was detected");
        }

        private static string DetectLocalSqlServerWithRetries(string connectionString, int retries)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                Logger.Info($"Probing for SQL instance using connection string {connectionString} with {retries} retries.");
                return Policy
                       .HandleResult<string>(r => r == null)
                       .WaitAndRetry(retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(retryAttempt, 3)) /* 1,9,27,64,125,216,343 secs */ )
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
                    using (SqlConnection connection = new SqlConnection(connectionString))
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

        public bool WaitUntilAvailable(int retries, Func<int, TimeSpan> sleepDurationProvider)
        {
            Logger.Info($"Probing for SQL instance with {retries} retries.");
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
            return Policy
                .HandleResult<bool>(result => result == false)
                .WaitAndRetry(retries, sleepDurationProvider)
                .Execute(() =>
                {
                    try
                    {
                        using (var connection = new SqlConnection(sb.ConnectionString))
                        {
                            connection.Open();
                            var command = connection.CreateCommand();
                            command.CommandText = "SELECT count(*) FROM sys.databases WHERE Name = 'master'";
                            command.ExecuteScalar();
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Info(ex, "No MSSQL instance was found");
                        return false;
                    }
                });
        }

        public async Task<bool> WaitUntilAvailableAsync(int retries, Func<int, TimeSpan> sleepDurationProvider, CancellationToken cancellationToken = default(CancellationToken))
        {
            Logger.Info($"Probing for SQL instance with {retries} retries.");
            var sb = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
            return await Policy
                .HandleResult<bool>(result => result == false)
                .WaitAndRetryAsync(retries, sleepDurationProvider)
                .ExecuteAsync(async () =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logger.Info("Waiting until database is available was cancelled");
                        return false;
                    }

                    try
                    {
                        using (var connection = new SqlConnection(sb.ConnectionString))
                        {
                            connection.Open();
                            var command = connection.CreateCommand();
                            command.CommandText = "SELECT count(*) FROM sys.databases WHERE Name = 'master'";
                            await command.ExecuteScalarAsync(cancellationToken);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Info(ex, "No MSSQL instance was found");
                        return false;
                    }
                });
        }

        public void EnsureExistingDatabase()
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(ConnectionString);
            string dbName = sb.InitialCatalog;
            sb.InitialCatalog = "master";
            using (var connection = new SqlConnection(sb.ConnectionString))
            {
                connection.Open();
                bool isExistant;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT count(*) FROM sys.databases WHERE Name = '{dbName}'";
                    isExistant = (int)command.ExecuteScalar() == 1;
                }

                if (!isExistant)
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
            using (SqlConnection connection = new SqlConnection(ConnectionString))
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
            }
        }

        public void CreateDatabase(string dbName)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var createCommand = connection.CreateCommand())
                {
                    createCommand.CommandText = $"CREATE DATABASE [{dbName}]";
                    createCommand.ExecuteNonQuery();
                }
            }
        }

        public void CreateSchema(string dbName, string createScriptContent)
        {
            // such scripts cannot be executed directly using ADO.Net, since it contains the batch delimeter "GO"
            // that is only understood by osql.exe or SQL Server Management Studio. Thus, we're splitting it
            // manually into commands now. (this is very naive, though)

            // depending on environment, git might have checked it out with windows or unix line breaks. This
            // replace will make sure that splitting works as designed.
            createScriptContent = createScriptContent.Replace("\r\n", "\n");

            string[] commandTexts = createScriptContent.Split(new[] { "\nGO\n" }, StringSplitOptions.RemoveEmptyEntries);
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var useCommand = connection.CreateCommand())
                {
                    useCommand.CommandText = $"USE [{dbName}]";
                    useCommand.ExecuteNonQuery();
                }

                foreach (string commandText in commandTexts)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}