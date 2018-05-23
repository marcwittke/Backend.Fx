namespace Backend.Fx.Testing.Containers
{
    using System;
    using System.Data;
    using System.IO;
    using System.Threading.Tasks;
    using Docker.DotNet.Models;
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using JetBrains.Annotations;

    public abstract class MssqlDockerContainer : DatabaseDockerContainer
    {
        private readonly string saPassword;

        protected MssqlDockerContainer(string dockerApiUrl, [NotNull] string saPassword, string name = null, string baseImage = null)
                : base(baseImage ?? "microsoft/mssql-server-linux:latest", name, new[] { $"SA_PASSWORD={saPassword}","ACCEPT_EULA=Y", "MSSQL_PID=Developer" }, dockerApiUrl)
        {
            this.saPassword = saPassword ?? throw new ArgumentNullException(nameof(saPassword));
        }

        protected override int DatabasePort { get; } = 1433;

        public override string ConnectionString
        {
            get
            {
                return $"Server=localhost,{LocalTcpPort};User=sa;Password={saPassword};";
            }
        }

        public override bool HealthCheck()
        {
            try
            {
                using (IDbConnection conn = CreateConnection())
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT 1";
                    cmd.ExecuteScalar();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task Restore(string bakFilePath, string dbName)
        {
            string targetPath = "/var/tmp";

            // the only possibility to copy something into the container is the method ExtractArchiveToContainer
            // so we have to provide the backup as tar/gzipped stream
            using (var stream = CreateTarGz(bakFilePath))
            {
                var parameters = new ContainerPathStatParameters { Path = targetPath, AllowOverwriteDirWithFile = true };
                await Client.Containers.ExtractArchiveToContainerAsync(ContainerId, parameters, stream);
            }

            using (IDbConnection connection = CreateConnection())
            {
                connection.Open();

                using (var restoreCommand = connection.CreateCommand())
                {
                    var restoreCommandCommandText
                            = $"RESTORE DATABASE [{dbName}] FROM  DISK = N'{targetPath}/{Path.GetFileName(bakFilePath)}' " +
                              "WITH FILE = 1, " +
                              $"MOVE N'mep-prod-sql_Data' TO N'/var/opt/mssql/data/{dbName}_data.mdf', " +
                              $"MOVE N'mep-prod-sql_Log' TO N'/var/opt/mssql/data/{dbName}_log.ldf', " +
                              "NOUNLOAD, REPLACE ";

                    restoreCommand.CommandText = restoreCommandCommandText;
                    restoreCommand.ExecuteNonQuery();
                }
            }
        }

        private Stream CreateTarGz(string sourceFile)
        {

            Stream outStream = new MemoryStream();
            using (Stream gzoStream = new GZipOutputStream(outStream))
            {
                using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream))
                {
                    TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceFile);
                    tarArchive.WriteEntry(tarEntry, true);
                    tarArchive.Close();
                }
            }

            outStream.Seek(0, SeekOrigin.Begin);
            return outStream;
        }
    }
}