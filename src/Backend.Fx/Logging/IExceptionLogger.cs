namespace Backend.Fx.Logging
{
    using System;
    using System.Linq;
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
                string[] clientErrorStrings = cex.Errors
                                                 .SelectMany(err => err.Value.Select(er => $"{Environment.NewLine}  {er.Code}:{er.Message}"))
                                                 .ToArray();

                var clientErrorString = string.Join("", clientErrorStrings);
                logger.Warn(cex, cex.Message + clientErrorString);
            }
            else
            {
                logger.Error(exception);
            }
        }
    }
}
