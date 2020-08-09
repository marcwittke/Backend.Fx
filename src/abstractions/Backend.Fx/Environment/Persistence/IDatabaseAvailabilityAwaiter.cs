using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Environment.Persistence
{
    public interface IDatabaseAvailabilityAwaiter
    {
        Task WaitForDatabase(CancellationToken cancellationToken);
    }
}