using System.Diagnostics;
using Backend.Fx.Logging;

namespace Backend.Fx.NetCore.Logging
{
    [DebuggerStepThrough]
    public class FrameworkToBackendFxLoggerFactory : Microsoft.Extensions.Logging.ILoggerFactory
    {
        public void Dispose() { }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new FrameworkToBackendFxLogger(LogManager.Create(categoryName));
        }

        public void AddProvider(Microsoft.Extensions.Logging.ILoggerProvider provider)
        { }
    }
}
