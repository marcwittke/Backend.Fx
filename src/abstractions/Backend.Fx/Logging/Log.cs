using System;
using System.Threading;
using Backend.Fx.Extensions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backend.Fx.Logging
{
    /// <summary>
    /// static class to keep an ILoggerFactory instance to use Microsoft.Extension.Logging without dependency injection
    /// </summary>
    public static class Log
    {
        private static Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory = new NullLoggerFactory();

        private static readonly AsyncLocal<Microsoft.Extensions.Logging.ILoggerFactory> AsyncLocalLoggerFactory =
            new AsyncLocal<Microsoft.Extensions.Logging.ILoggerFactory>();

        public static void Initialize(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Override the global, static ILoggerFactory in this async local scope. This can be done per web request or per test run
        /// </summary>
        /// <param name="loggerFactory"></param>
        public static IDisposable InitAsyncLocal(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            AsyncLocalLoggerFactory.Value = loggerFactory;
            return new DelegateDisposable(() => AsyncLocalLoggerFactory.Value = null);
        }

        public static Microsoft.Extensions.Logging.ILogger Create<T>()
        {
            return GetLoggerFactory().CreateLogger(typeof(T).FullName);
        }

        public static Microsoft.Extensions.Logging.ILogger Create(Type t)
        {
            return GetLoggerFactory().CreateLogger(t.FullName);
        }

        public static Microsoft.Extensions.Logging.ILogger Create(string category)
        {
            return GetLoggerFactory().CreateLogger(category);
        }

        private static Microsoft.Extensions.Logging.ILoggerFactory GetLoggerFactory()
        {
            return AsyncLocalLoggerFactory.Value ?? _loggerFactory;
        }
    }

}