using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Features.Persistence.AdoNet;

namespace Backend.Fx.EfCore6Persistence.Tests.Fixtures;

public abstract class TestDatabase : IDatabaseBootstrapper, IDbConnectionFactory
{
    public abstract string ConnectionString { get; }

   public abstract IDbConnection Create();

    public abstract Task EnsureDatabaseExistenceAsync(CancellationToken cancellationToken);

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}