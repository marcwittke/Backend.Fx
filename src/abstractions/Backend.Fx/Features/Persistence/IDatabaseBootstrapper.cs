using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Features.Persistence
{
    /// <summary>
    /// Encapsulates database bootstrapping. This interface hides the implementation details for creating/migrating the database
    /// </summary>
    public interface IDatabaseBootstrapper : IDisposable
    {
        Task EnsureDatabaseExistenceAsync(CancellationToken cancellationToken);
    }
}