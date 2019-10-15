using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Environment.Persistence
{
    public interface IDatabaseUtil
    {
        bool WaitUntilAvailable(int retries, Func<int, TimeSpan> sleepDurationProvider);
        Task<bool> WaitUntilAvailableAsync(int retries, Func<int, TimeSpan> sleepDurationProvider, CancellationToken cancellationToken = default(CancellationToken));
    }
}