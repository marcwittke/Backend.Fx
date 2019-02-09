using System;

namespace Backend.Fx.Environment.Persistence
{
    public interface IDatabaseUtil
    {
        bool WaitUntilAvailable(int retries, Func<int, TimeSpan> sleepDurationProvider);
    }
}