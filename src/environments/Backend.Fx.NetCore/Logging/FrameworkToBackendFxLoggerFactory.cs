using System.Diagnostics;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Backend.Fx.NetCore.Logging
{
    [DebuggerStepThrough]
    public class FrameworkToBackendFxLoggerFactory : ILoggerFactory
    {
        public void Dispose()
        { }

        public ILogger CreateLogger(string categoryName)
        {
            return new FrameworkToBackendFxLogger(LogManager.Create(categoryName));
        }

        public void AddProvider(ILoggerProvider provider)
        { }
    }
}
