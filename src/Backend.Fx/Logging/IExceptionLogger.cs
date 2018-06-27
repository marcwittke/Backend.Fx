namespace Backend.Fx.Logging
{
    using System;
    using Exceptions;

    public interface IExceptionLogger
    {
        void LogException(Exception exception);
    }

    public class ExceptionLogger : IExceptionLogger
    {
        private readonly ILogger logger;

        public ExceptionLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void LogException(Exception exception)
        {
            if (exception is ClientException cex)
            {
                logger.Warn(cex, cex.Message + Environment.NewLine + cex.Errors);
            }
            else
            {
                logger.Error(exception);
            }
        }
    }
}
