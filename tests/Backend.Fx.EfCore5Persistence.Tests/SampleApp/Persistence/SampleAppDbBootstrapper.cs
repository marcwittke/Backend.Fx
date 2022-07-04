using Backend.Fx.Features.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore5Persistence.Tests.SampleApp.Persistence
{
    public class SampleAppDbBootstrapper : IDatabaseBootstrapper
    {
        private readonly string _connectionString;

        public SampleAppDbBootstrapper(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void Dispose()
        { }

        public void EnsureDatabaseExistence()
        {
            var dbContext = new SampleAppDbContext(
                new DbContextOptionsBuilder<SampleAppDbContext>().UseSqlite(_connectionString).Options);
            dbContext.Database.EnsureCreated();
        }
    }
}