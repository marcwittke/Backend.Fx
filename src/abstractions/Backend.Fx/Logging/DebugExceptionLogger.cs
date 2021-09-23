using System;
using System.Diagnostics;
using Backend.Fx.Exceptions;

namespace Backend.Fx.Logging
{
    public class DebugExceptionLogger : IExceptionLogger
    {
        public void LogException(Exception exception)
        {
            if (exception is ClientException cex)
            {
                Debug.WriteLine(cex + System.Environment.NewLine + cex.Errors);
            }
            else
            {
                Debug.WriteLine(exception);
            }
        }
    }
}
