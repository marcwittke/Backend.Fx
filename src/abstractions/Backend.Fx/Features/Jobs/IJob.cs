using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.Features.Jobs
{
    /// <summary>
    /// This interface describes a job that can be executed directly or by a scheduler.
    /// </summary>
    [PublicAPI]
    public interface IJob
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}