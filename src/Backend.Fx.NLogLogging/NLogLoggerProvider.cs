namespace Backend.Fx.NLogLogging
{
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    [DebuggerStepThrough]
    public class NLogLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string name)
        {
            return new FrameworkToNLogLogger(NLog.LogManager.GetLogger(name));
        }

        public void Dispose() { }
    }
}