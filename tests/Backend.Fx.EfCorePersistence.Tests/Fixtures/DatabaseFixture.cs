using System.Data;
using System.Security.Principal;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
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

        public abstract DbContextOptionsBuilder<TestDbContext> GetDbContextOptionsBuilder(IDbConnection connection);

        public abstract DbConnectionOperationDecorator UseOperation();

        public TestDbSession CreateTestDbSession(
            DbConnectionOperationDecorator operation = null,
            IIdentity asIdentity = null,
            IClock clock = null)
        {
            CurrentIdentityHolder CreateAsIdentity()
            {
                var cih = new CurrentIdentityHolder();
                cih.ReplaceCurrent(asIdentity);
                return cih;
            }

            clock ??= new WallClock();
            operation ??= UseOperation();

            operation.Begin();

            ICurrentTHolder<IIdentity> identityHolder = asIdentity == null
                ? CurrentIdentityHolder.CreateSystem()
                : CreateAsIdentity();
            return new TestDbSession(this, operation, identityHolder, clock);
        }
    }
}
