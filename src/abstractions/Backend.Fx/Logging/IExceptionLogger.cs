using System;
using Backend.Fx.Exceptions;

namespace Backend.Fx.Logging
{
    public interface IExceptionLogger
    {
        void LogException(Exception exception);
    }


    public class ExceptionLogger : IExceptionLogger
    {
        private readonly ILogger _logger;

        public ExceptionLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogException(Exception exception)
        {
            if (exception is ClientException cex)
            {
                _logger.Warn(cex);
            }
            else
            {
                _logger.Error(exception);
            }
        }
    }
}
