using System;
using System.Data;
using System.Security.Principal;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class DbSession : IDisposable
    {
        public DbSession(IDbConnection connection, DbContextOptions<TestDbContext> options)
        {
            DbContext = new TestDbContext(options);
            Connection = connection;
            
            Assert.Equal(Connection, DbContext.Database.GetDbConnection());
        }

        public TestDbContext DbContext { get; }

        public IDbConnection Connection { get; }
        
        public IUnitOfWork UseUnitOfWork(IClock clock = null, IIdentity identity = null)
        {
            ICurrentTHolder<IIdentity> currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(identity ?? new SystemIdentity());
            var uow = new EfUnitOfWork(clock ?? new FrozenClock(), currentIdentityHolder, A.Fake<IDomainEventAggregator>(), A.Fake<IEventBusScope>(), DbContext, Connection);
            uow.Begin();
            return uow;
        }
        
        public void Dispose()
        {
            Connection.Close();
        }
    }
}