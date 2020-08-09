using System.Data.Common;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    public abstract class DatabaseFixture
    {
        public void CreateDatabase()
        {
            using (var dbContext = new TestDbContext(GetDbContextOptionsForDbCreation()))
            {
                dbContext.Database.EnsureCreated();
            }
        }

        protected abstract DbContextOptions<TestDbContext> GetDbContextOptionsForDbCreation();

        protected abstract DbContextOptionsBuilder<TestDbContext> GetDbContextOptionsBuilder(DbConnection connection);

        public abstract DbSession UseDbSession();
    }
}