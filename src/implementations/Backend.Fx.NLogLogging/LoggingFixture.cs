using System;
using System.Reflection;

namespace Backend.Fx.NLogLogging
{
    /// <summary>
    /// To be implemented and used as AssemblyFixture
    /// Configures NLog and shuts down logging after test execution
    /// </summary>
    public abstract class LoggingFixture : IDisposable
    {
        private static IDisposable _lifetimeLogger;

        protected LoggingFixture(string appRootNamespace)
        {
            Configurations.ForTests(appRootNamespace, GetType().GetTypeInfo().Assembly.GetName().Name + ".xlog");

            _lifetimeLogger = Backend.Fx.Logging.LogManager.Create<LoggingFixture>().InfoDuration("Test run started", "Test run finished");
        }

        public void Dispose()
        {
            _lifetimeLogger.Dispose();
            NLog.LogManager.Shutdown();
        }
    }
}
