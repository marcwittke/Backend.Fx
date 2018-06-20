namespace Backend.Fx.Testing.Logging
{
    using System;

    /// <summary>
    /// To be implemented and used as AssemblyFixture
    /// Configures NLog and shuts down logging after test execution
    /// </summary>
    public abstract class LoggingFixture : IDisposable
    {
        private static IDisposable lifetimeLogger;

        protected LoggingFixture(string appRootNamespace)
        {
            Configurations.ForTests(appRootNamespace, GetType().Assembly.GetName().Name + ".xlog");

            lifetimeLogger = Backend.Fx.Logging.LogManager.Create<LoggingFixture>().InfoDuration("Test run started", "Test run finished");
        }

        public void Dispose()
        {
            lifetimeLogger.Dispose();
            NLog.LogManager.Shutdown();
        }
    }
}
