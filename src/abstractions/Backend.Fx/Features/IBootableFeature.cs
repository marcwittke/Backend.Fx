using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Features
{
    /// <summary>
    /// Marks a <see cref="Feature"/> to require stuff done during startup of the application
    /// </summary>
    public interface IBootableFeature
    {
        public Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default);
    }
}