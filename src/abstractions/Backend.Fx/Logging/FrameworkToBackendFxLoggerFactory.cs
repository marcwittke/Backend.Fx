using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Logging
{
    [DebuggerStepThrough]
    public class FrameworkToBackendFxLoggerFactory : Microsoft.Extensions.Logging.ILoggerFactory
    {
        public void Dispose()
        { }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new FrameworkToBackendFxLogger(LogManager.Create(categoryName));
        }

        public void AddProvider(ILoggerProvider provider)
        { }
    }
}
