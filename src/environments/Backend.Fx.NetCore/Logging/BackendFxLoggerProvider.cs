using System.Diagnostics;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.NetCore.Logging
{
    [DebuggerStepThrough]
    public class BackendFxLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string name)
        {
            return new FrameworkToBackendFxLogger(LogManager.Create(name));
        }

        public void Dispose()
        { }
    }
}
