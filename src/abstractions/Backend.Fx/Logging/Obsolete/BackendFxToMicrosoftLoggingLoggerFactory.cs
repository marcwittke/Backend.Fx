using System;

// ReSharper disable CheckNamespace
namespace Backend.Fx.Logging
{
    [Obsolete]
    public class BackendFxToMicrosoftLoggingLoggerFactory : ILoggerFactory
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        public BackendFxToMicrosoftLoggingLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public ILogger Create(string s)
        {
            return new BackendFxToMicrosoftLoggingLogger(_loggerFactory.CreateLogger(s));
        }

        public ILogger Create(Type t)
        {
            return new BackendFxToMicrosoftLoggingLogger(_loggerFactory.CreateLogger(t.FullName));
        }

        public ILogger Create<T>()
        {
            return new BackendFxToMicrosoftLoggingLogger(_loggerFactory.CreateLogger(typeof(T).FullName));
        }

        public void BeginActivity(int activityIndex)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}