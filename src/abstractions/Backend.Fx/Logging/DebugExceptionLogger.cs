namespace Backend.Fx.Logging
{
    using System;
    using System.Diagnostics;

    public class DebugExceptionLogger : IExceptionLogger
    {
        public void LogException(Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }
}