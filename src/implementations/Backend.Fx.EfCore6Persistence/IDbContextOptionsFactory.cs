using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence;

public interface IDbContextOptionsFactory<TDbContext> where TDbContext : DbContext
{
    DbContextOptions<TDbContext> GetDbContextOptions(IDbConnection dbConnection);
}