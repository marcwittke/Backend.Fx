namespace Backend.Fx.NLogLogging
{
    using System;
    using System.IO;
    using NLog;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;

    /// <summary>
    /// To be used in tests
    /// </summary>
    public class NLogLoggingFixture
    {
        private static bool isConfigured;
        public NLogLoggingFixture()
        {
            lock (this)
            {
                if (!isConfigured)
                {
                    ConfigureNLogForTests();
                    isConfigured = true;
                }
            }
        }

        public static void ConfigureNLogForTests()
        {
            const string logfilename = "${shortdate}.xlog";
            Backend.Fx.Logging.LogManager.Initialize(new NLogLoggerFactory());
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = @"${level:uppercase=true:padding=5} ${mdc:item=Activity} ${time} ${logger} ${message} ${exception}"
            };
            config.AddTarget("console", consoleTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Warn, consoleTarget));
            
            var fileTarget = new FileTarget
            {
                DeleteOldFileOnStartup = true,
                FileName = @"${basedir}/" + logfilename,
                Layout = new Log4JXmlEventLayout(),
                MaxArchiveFiles = 1,
            };
            config.AddTarget("file", fileTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, fileTarget));

            NLog.LogManager.Configuration = config;

            Console.WriteLine($"Test console shows only warn, error and fatal events. Full log file is available at {Path.Combine(AppContext.BaseDirectory, logfilename)}");
        }
    }
}
