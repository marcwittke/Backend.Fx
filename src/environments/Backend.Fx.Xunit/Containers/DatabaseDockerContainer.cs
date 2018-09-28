using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Backend.Fx.Xunit.Containers
{
    public abstract class DatabaseDockerContainer : DockerContainer
    {
        private readonly string[] _env;

        protected DatabaseDockerContainer(string baseImage, string name, string[] env, string dockerApiUrl)
                : base(baseImage, name, dockerApiUrl)
        {
            _env = env;
        }

        protected abstract int DatabasePort { get; }

        protected int LocalTcpPort { get; } = TcpPorts.GetUnused();

        public abstract string ConnectionString { get; }

        protected override CreateContainerParameters CreateParameters => new CreateContainerParameters
        {
            Image = BaseImage,
            AttachStderr = true,
            AttachStdin = true,
            AttachStdout = true,
            Env = _env,
            ExposedPorts = new Dictionary<string, EmptyStruct> { { DatabasePort.ToString(CultureInfo.InvariantCulture), new EmptyStruct() } },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        DatabasePort.ToString(CultureInfo.InvariantCulture),
                        new List<PortBinding> {new PortBinding {HostPort = LocalTcpPort.ToString(CultureInfo.InvariantCulture)}}
                    }
                }
            },
            Name = Name,
        };

        public abstract IDbConnection CreateConnection();

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync();
            TcpPorts.Free(LocalTcpPort);
        }
    }
}