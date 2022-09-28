using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Features.Jobs
{
    /// <summary>
    /// This interface describes a job that can be executed directly or by a scheduler.
    /// </summary>
    public interface IJob
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}