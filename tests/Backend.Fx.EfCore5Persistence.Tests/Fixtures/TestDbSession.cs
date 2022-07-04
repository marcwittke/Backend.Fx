using System;
using System.Data;
using System.Security.Principal;
using Backend.Fx.EfCore5Persistence.Tests.SampleApp.Persistence;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.EfCore5Persistence.Tests.Fixtures
{
    public class TestDbSession : ICanFlush, IDisposable
    {
        private readonly DbConnectionOperationDecorator _operation;
        private readonly EfFlush _efFlush;

        public TestDbSession(DatabaseFixture fixture, DbConnectionOperationDecorator operation, ICurrentTHolder<IIdentity> identityHolder, IClock clock)
        {
            _operation = operation;
            DbContext = new SampleAppDbContext(fixture.GetDbContextOptionsBuilder(operation.DbConnection).Options);
            _efFlush = new EfFlush(DbContext, identityHolder, clock);
            DbConnection = operation.DbConnection;
        }


        public SampleAppDbContext DbContext { get; }
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