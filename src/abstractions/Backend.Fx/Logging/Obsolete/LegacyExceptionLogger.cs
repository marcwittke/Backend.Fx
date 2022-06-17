using System;
using Backend.Fx.Exceptions;

// ReSharper disable CheckNamespace
namespace Backend.Fx.Logging
{
    [Obsolete]
    public class LegacyExceptionLogger : IExceptionLogger
    {
        private readonly ILogger _logger;

        public LegacyExceptionLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogException(Exception exception)
        {
            if (exception is ClientException cex)
            {
                _logger.Warn(cex, "Client Exception");
            }
            else
            {
                _logger.Error(exception, "Server Exception");
            }
        }
    }
}