using System;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;
using ILogger = Serilog.ILogger;

namespace Backend.Fx.Tests
{
    public abstract class TestWithLogging : IDisposable
    {
        private readonly IDisposable _disposableLogger;

        protected TestWithLogging(ITestOutputHelper output)
        {
            var loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.TestOutput(output);
            Logger = loggerConfiguration.CreateLogger();
            _disposableLogger = Logging.Log.InitAsyncLocal(new SerilogLoggerFactory(Logger));
        }

        protected ILogger Logger { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposableLogger?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}