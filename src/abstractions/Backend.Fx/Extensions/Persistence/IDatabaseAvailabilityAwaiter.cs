using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Extensions.Persistence
{
    public interface IDatabaseAvailabilityAwaiter
    {
        Task WaitForDatabase(CancellationToken cancellationToken);
    }
}