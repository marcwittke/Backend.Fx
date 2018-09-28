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
        private readonly string _saPassword;

        protected MssqlDockerContainer(string dockerApiUrl, [NotNull] string saPassword, string name = null, string baseImage = null)
                : base(baseImage ?? "microsoft/mssql-server-linux:latest", name, new[] { $"SA_PASSWORD={saPassword}", "ACCEPT_EULA=Y", "MSSQL_PID=Developer" }, dockerApiUrl)
        {
            this._saPassword = saPassword ?? throw new ArgumentNullException(nameof(saPassword));
        }

        protected override int DatabasePort { get; } = 1433;

        public override string ConnectionString
        {
            get
            {
                return $"Server=localhost,{LocalTcpPort};User=sa;Password={_saPassword};";
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
                    restoreCommand.CommandText = "USE master";
                    restoreCommand.ExecuteNonQuery();
                }

                string logicalDataName = "";
                string logicalLogName = "";
                using (var fileListCommand=connection.CreateCommand())
                {
                    fileListCommand.CommandText = $"RESTORE FILELISTONLY FROM DISK = N'{targetPath}/{Path.GetFileName(bakFilePath)}'";
                    using (var reader = fileListCommand.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            int typeOrdinal = reader.GetOrdinal("Type");
                            int logicalNameOrdinal = reader.GetOrdinal("LogicalName");
                            if (reader.GetString(typeOrdinal) == "D")
                            {
                                logicalDataName = reader.GetString(logicalNameOrdinal);
                            }

                            if (reader.GetString(typeOrdinal) == "L")
                            {
                                logicalLogName = reader.GetString(logicalNameOrdinal);
                            }
                        }
                    }
                }

                using (var restoreCommand = connection.CreateCommand())
                {
                    var restoreCommandCommandText
                            = $"RESTORE DATABASE [{dbName}] FROM  DISK = N'{targetPath}/{Path.GetFileName(bakFilePath)}' " +
                              "WITH FILE = 1, " +
                              $"MOVE N'{logicalDataName}' TO N'/var/opt/mssql/data/{dbName}_data.mdf', " +
                              $"MOVE N'{logicalLogName}' TO N'/var/opt/mssql/data/{dbName}_log.ldf', " +
                              "NOUNLOAD, REPLACE ";

                    restoreCommand.CommandText = restoreCommandCommandText;
                    restoreCommand.ExecuteNonQuery();
                }
            }
        }

        private static Stream CreateTarGz(string sourceFile)
        {
            var tempFileName = Path.GetTempFileName();
            Stream outStream = File.Create(tempFileName);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);
    
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceFile);
            tarArchive.WriteEntry(tarEntry, true);
    
            tarArchive.Close();

            return File.OpenRead(tempFileName);
        }
    }
}