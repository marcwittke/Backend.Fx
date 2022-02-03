using System;
using Backend.Fx.Exceptions;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Logging
{
    public interface IExceptionLogger
    {
        void LogException(Exception exception);
    }

    public class ExceptionLogger : IExceptionLogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public ExceptionLogger(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public void LogException(Exception exception)
        {
            if (exception is ClientException cex)
            {
                _logger.LogWarning(cex, "Client Exception");
            }
            else
            {
                _logger.LogError(exception, "Server Exception");
            }
        }
    }
}