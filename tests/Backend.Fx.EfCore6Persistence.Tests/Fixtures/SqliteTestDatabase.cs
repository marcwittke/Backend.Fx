using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Backend.Fx.EfCore6Persistence.Tests.Fixtures;

public class SqliteTestDatabase : TestDatabase
{
    private readonly string _fileName = Path.GetTempFileName();

    public override string ConnectionString => $"Data Source={_fileName};";

    public override IDbConnection Create()
    {
        return new SqliteConnection(ConnectionString);
    }

    public override async Task EnsureDatabaseExistenceAsync(CancellationToken cancellationToken)
    {
        await using var dbConnection = new SqliteConnection(ConnectionString);
        await dbConnection.OpenAsync(cancellationToken);
        await dbConnection.CloseAsync();
    }
}