using System.Data;
using System.Security.Principal;
using Backend.Fx.EfCore5Persistence.Tests.SampleApp.Persistence;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Extensions.Persistence;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Backend.Fx.EfCore5Persistence.Tests.Fixtures
{
    public abstract class DatabaseFixture
    {
        public void CreateDatabase()
        {
            using (var dbContext = new SampleAppDbContext(GetDbContextOptionsForDbCreation()))
            {
                dbContext.Database.EnsureCreated();
            }
        }

        protected abstract DbContextOptions<SampleAppDbContext> GetDbContextOptionsForDbCreation();

        public abstract DbContextOptionsBuilder<SampleAppDbContext> GetDbContextOptionsBuilder(IDbConnection connection);

        public abstract DbConnectionOperationDecorator UseOperation();

        public TestDbSession CreateTestDbSession(DbConnectionOperationDecorator operation = null, IIdentity asIdentity = null, IClock clock = null)
        {
            CurrentIdentityHolder CreateAsIdentity()
            {
                var cih = new CurrentIdentityHolder();
                cih.ReplaceCurrent(asIdentity);
                return cih;
            }

            clock ??= SystemClock.Instance;
            operation ??= UseOperation();
            
            operation.Begin();

            var identityHolder = asIdentity == null
                                     ? CurrentIdentityHolder.CreateSystem()
                                     : CreateAsIdentity();
            return new TestDbSession(this, operation, identityHolder, clock);
        }
    }
}