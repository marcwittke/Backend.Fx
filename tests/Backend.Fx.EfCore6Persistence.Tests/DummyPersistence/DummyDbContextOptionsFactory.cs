using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyPersistence;

public class DummyDbContextOptionsFactory : IDbContextOptionsFactory<DummyDbContext>
{
    public DbContextOptions<DummyDbContext> GetDbContextOptions(IDbConnection dbConnection)
    {
        return new DbContextOptionsBuilder<DummyDbContext>()
            .UseSqlite((SqliteConnection)dbConnection, opt => opt.UseNodaTime())
            .Options;
    }
}