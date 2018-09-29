using Backend.Fx.Exceptions;

namespace Backend.Fx.Logging
{
    using System;
    using System.Diagnostics;

    public class DebugExceptionLogger : IExceptionLogger
    {
        public void LogException(Exception exception)
        {
            if (exception is ClientException cex)
            {
                Debug.WriteLine(cex + Environment.NewLine + cex.Errors);
            }
            else
            {
                Debug.WriteLine(exception);
            }
        }
    }
}