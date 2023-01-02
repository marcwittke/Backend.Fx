using System;
using System.Threading;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backend.Fx.Logging
{
    /// <summary>
    /// static class to keep an ILoggerFactory instance to use Microsoft.Extension.Logging without dependency injection
    /// </summary>
    [PublicAPI]
    public static class Log
    {
        private static ILoggerFactory _loggerFactory = new NullLoggerFactory();

        private static readonly AsyncLocal<ILoggerFactory> AsyncLocalLoggerFactory = new();

        public static void Initialize(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Override the global, static ILoggerFactory in this async local scope. This can be done per web request or per test run
        /// </summary>
        /// <param name="loggerFactory"></param>
        public static IDisposable InitAsyncLocal(ILoggerFactory loggerFactory)
        {
            AsyncLocalLoggerFactory.Value = loggerFactory;
            return new DelegateDisposable(() => AsyncLocalLoggerFactory.Value = null);
        }

        public static ILogger Create<T>()
        {
            return LoggerFactory.CreateLogger(typeof(T).FullName);
        }

        public static ILogger Create(Type t)
        {
            return LoggerFactory.CreateLogger(t.FullName);
        }

        public static ILogger Create(string category)
        {
            return LoggerFactory.CreateLogger(category);
        }

        public static ILoggerFactory LoggerFactory { get; } = new MaybeAsyncLocalLoggerFactory();

        private class MaybeAsyncLocalLoggerFactory : ILoggerFactory
        {
            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName)
            {
                return (AsyncLocalLoggerFactory.Value ?? _loggerFactory).CreateLogger(categoryName);
            }

            public void AddProvider(ILoggerProvider provider)
            {
                (AsyncLocalLoggerFactory.Value ?? _loggerFactory).AddProvider(provider);
            }
        }
    }
}
