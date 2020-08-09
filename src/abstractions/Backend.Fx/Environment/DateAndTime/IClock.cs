using System;

namespace Backend.Fx.Environment.DateAndTime
{
    /// <summary>
    /// Wraps access to DateTime.UtcNow. By means of this interface the current time can be mocked.
    /// the database should only store universal date and time values, that could be translated into user's time by applying a UtcOffset
    /// </summary>
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}