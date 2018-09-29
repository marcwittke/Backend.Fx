using System.Diagnostics;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.NetCore.Logging
{
    [DebuggerStepThrough]
    public class BackendFxLoggerProvider : ILoggerProvider
    {
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string name)
        {
            return new FrameworkToBackendFxLogger(LogManager.Create(name));
        }

        public void Dispose() { }
    }
}