using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Backend.Fx.NLogLogging
{
    public static class Configurations
    {
        private static readonly object SyncLock = new object();

        public static void ForTests(string appRootNamespace, string logfilename = "tests.log")
        {
            lock (SyncLock)
            {
                if (LogManager.Configuration != null) return;

                Logging.LogManager.Initialize(new NLogLoggerFactory());
                var config = new LoggingConfiguration();

                var consoleTarget = new ConsoleTarget
                {
                    Layout = @"${level:uppercase=true:padding=5} ${mdc:item=Activity} ${time} ${logger} ${message} ${exception}"
                };
                config.AddTarget("console", consoleTarget);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Warn, consoleTarget));
                config.LoggingRules.Add(new LoggingRule("Xunit.*", LogLevel.Info, consoleTarget));

                var fileTarget = new FileTarget
                {
                    DeleteOldFileOnStartup = false,
                    FileName = @"${basedir}/" + logfilename,
                    Layout = @"${level:uppercase=true:padding=5} ${mdc:item=Activity} ${time} ${logger} ${message} ${exception}",
                    MaxArchiveFiles = 1,
                    ArchiveAboveSize = 10 * 1024 * 1024
                };
                config.AddTarget("file", fileTarget);
                config.LoggingRules.Add(new LoggingRule(appRootNamespace + ".*", LogLevel.Debug, fileTarget));
                config.LoggingRules.Add(new LoggingRule("Xunit.*", LogLevel.Info, fileTarget));
                config.LoggingRules.Add(new LoggingRule("Microsoft.*", LogLevel.Warn, fileTarget));
                config.LoggingRules.Add(new LoggingRule("Microsoft.AspNetCore.Hosting.Internal.WebHost", LogLevel.Info, fileTarget));
                config.LoggingRules.Add(new LoggingRule("Backend.*", LogLevel.Info, fileTarget));

                var fatals = new MemoryTarget(LogLevel.Fatal.ToString())
                {
                    Layout = @"${level:uppercase=true:padding=5} ${mdc:item=Activity} ${time} ${logger} ${message} ${exception}"
                };
                config.AddTarget(fatals);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Fatal, LogLevel.Fatal, fatals));

                var errors = new MemoryTarget(LogLevel.Error.ToString())
                {
                    Layout = @"${level:uppercase=true:padding=5} ${mdc:item=Activity} ${time} ${logger} ${message} ${exception}"
                };
                config.AddTarget(errors);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, LogLevel.Error, errors));

                var warnings = new MemoryTarget(LogLevel.Warn.ToString())
                {
                    Layout = @"${level:uppercase=true:padding=5} ${mdc:item=Activity} ${time} ${logger} ${message} ${exception}"
                };
                config.AddTarget(warnings);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Warn, LogLevel.Warn, warnings));

                LogManager.Configuration = config;

                Console.WriteLine($"Test console shows only warn, error and fatal events. Full log file is available at {Path.Combine(AppContext.BaseDirectory, logfilename)}");
            }
        }

        public static IEnumerable<string> LogMessages(LogLevel level)
        {
            return LogMessages(level.ToString());
        }

        public static IEnumerable<string> LogMessages(string level)
        {
            return LogManager.Configuration?.FindTargetByName<MemoryTarget>(level)?.Logs ?? Enumerable.Empty<string>();
        }
    }
}