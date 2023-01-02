using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Features.Persistence
{
    public interface IDatabaseAvailabilityAwaiter
    {
        Task WaitForDatabase(CancellationToken cancellationToken);
    }
}