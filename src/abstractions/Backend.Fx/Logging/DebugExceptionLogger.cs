using System;
using System.Diagnostics;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;

namespace Backend.Fx.Logging
{
    [PublicAPI]
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