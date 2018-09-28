namespace Backend.Fx.Logging
{
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

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