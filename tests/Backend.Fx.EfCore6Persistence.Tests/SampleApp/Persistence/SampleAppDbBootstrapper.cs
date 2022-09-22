using Backend.Fx.Extensions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence.Tests.SampleApp.Persistence
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
                new DbContextOptionsBuilder<SampleAppDbContext>().UseSqlite(_connectionString, opt => opt.UseNodaTime()).Options);
            dbContext.Database.EnsureCreated();
        }
    }
}