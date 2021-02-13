using System;
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

        public TestDbSession CreateTestDbSession(DbConnectionOperationDecorator operation = null, IIdentity asIdentity = null, IClock clock = null)
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

            var identityHolder = asIdentity == null
                                     ? CurrentIdentityHolder.CreateSystem()
                                     : CreateAsIdentity();
            return new TestDbSession(this, operation, identityHolder, clock);
        }
    }

    public class TestDbSession : ICanFlush, IDisposable
    {
        private readonly DbConnectionOperationDecorator _operation;
        private readonly DbSession _dbSession;

        public TestDbSession(DatabaseFixture fixture, DbConnectionOperationDecorator operation, ICurrentTHolder<IIdentity> identityHolder, IClock clock)
        {
            _operation = operation;
            DbContext = new TestDbContext(fixture.GetDbContextOptionsBuilder(operation.DbConnection).Options);
            _dbSession = new DbSession(DbContext, identityHolder, clock);
            DbConnection = operation.DbConnection;
        }


        public TestDbContext DbContext { get; }
        public IDbConnection DbConnection { get; }

        public void Flush()
        {
            _dbSession.Flush();
        }

        public void Dispose()
        {
            _dbSession.Flush();
            DbContext.Dispose();
            _operation.Complete();
        }
    }
}