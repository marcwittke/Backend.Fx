using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Features.Persistence.AdoNet;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyPersistence;

public class DummyDatabaseBootstrapper : IDatabaseBootstrapper
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IDbContextOptionsFactory<DummyDbContext> _dbContextOptionsFactory;

    public DummyDatabaseBootstrapper(IDbConnectionFactory dbConnectionFactory, IDbContextOptionsFactory<DummyDbContext> dbContextOptionsFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _dbContextOptionsFactory = dbContextOptionsFactory;
    }

    public void Dispose()
    {
    }

    public async Task EnsureDatabaseExistenceAsync(CancellationToken cancellationToken)
    {
        using IDbConnection dbConnection = _dbConnectionFactory.Create();
        await using var dbContext = new DummyDbContext(
            new CurrentTenantIdHolder(),
            _dbContextOptionsFactory.GetDbContextOptions(dbConnection));
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}