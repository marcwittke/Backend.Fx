
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Backend.Fx.Logging;
using Backend.Fx.RandomData;

namespace Backend.Fx.Xunit.Containers
{
    public class TcpPorts
    {
        private static readonly HashSet<int> UsedPorts = new HashSet<int>(IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port));
        private static readonly ILogger Logger = LogManager.Create<TcpPorts>();

        public static int GetUnused()
        {
            lock (UsedPorts)
            {
                var unusedPorts = Enumerable.Range(50000, 14000).Except(UsedPorts);
                var unusedPort = unusedPorts.Random();
                UsedPorts.Add(unusedPort);
                return unusedPort;
            }
        }

        public static void Free(int tcpPort)
        {
            lock (UsedPorts)
            {
                UsedPorts.Remove(tcpPort);
            }
        }
    }
}
