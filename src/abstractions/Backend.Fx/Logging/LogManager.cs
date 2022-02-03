using System;

namespace Backend.Fx.Logging
{
    /// <summary>
    /// static class to keep an ILoggerFactory instance to use Microsoft.Extension.Logging without dependency injection
    /// </summary>
    public static class LogManager
    {
        private static Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        public static void Init(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public static Microsoft.Extensions.Logging.ILogger Create<T>()
        {
            return _loggerFactory.CreateLogger(typeof(T).FullName);
        }
        
        public static Microsoft.Extensions.Logging.ILogger Create(Type t)
        {
            return _loggerFactory.CreateLogger(t.FullName);
        }
        
        public static Microsoft.Extensions.Logging.ILogger Create(string category)
        {
            return _loggerFactory.CreateLogger(category);
        }
        
        public static void BeginActivity() {}

        public static void Shutdown()
        {
            _loggerFactory.Dispose();
        }
    }
    
}