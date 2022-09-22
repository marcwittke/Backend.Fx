using System;

namespace Backend.Fx.Extensions.Persistence
{
    /// <summary>
    /// Encapsulates database bootstrapping. This interface hides the implementation details for creating/migrating the database
    /// </summary>
    public interface IDatabaseBootstrapper : IDisposable
    {
        void EnsureDatabaseExistence();
    }
}