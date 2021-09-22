using System;
using System.Data;
using System.Security.Principal;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    public class TestDbSession : ICanFlush, IDisposable
    {
        private readonly EfFlush _efFlush;
        private readonly DbConnectionOperationDecorator _operation;

        public TestDbSession(
            DatabaseFixture fixture,
            DbConnectionOperationDecorator operation,
            ICurrentTHolder<IIdentity> identityHolder,
            IClock clock)
        {
            _operation = operation;
            DbContext = new TestDbContext(fixture.GetDbContextOptionsBuilder(operation.DbConnection).Options);
            _efFlush = new EfFlush(DbContext, identityHolder, clock);
            DbConnection = operation.DbConnection;
        }

        public TestDbContext DbContext { get; }

        public IDbConnection DbConnection { get; }

        public void Flush()
        {
            _efFlush.Flush();
        }

        public void Dispose()
        {
            _efFlush.Flush();
            DbContext.Dispose();
            _operation.Complete();
        }
    }
}
