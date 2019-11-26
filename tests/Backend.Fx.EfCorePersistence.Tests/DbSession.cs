using System;
using System.Data.Common;
using System.Security.Principal;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class DbSession : IDisposable
    {
        public DbSession(DbConnection connection, DbContextOptionsBuilder<TestDbContext> optionsBuilder)
        {
            Connection = connection;
            OptionsBuilder = optionsBuilder;
            connection.Open();
        }

        public DbConnection Connection { get; }
        public DbContextOptionsBuilder<TestDbContext> OptionsBuilder { get; }

        public EfUnitOfWork<TestDbContext> BeginUnitOfWork(IClock clock = null, IIdentity identity = null)
        {
            ICurrentTHolder<IIdentity> currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(identity ?? new SystemIdentity());
            var uow = new EfUnitOfWork<TestDbContext>(clock ?? new FrozenClock(),
                currentIdentityHolder,
                A.Fake<IDomainEventAggregator>(),
                A.Fake<IEventBusScope>(),
                connection => new TestDbContext(OptionsBuilder.Options),
                Connection);
            uow.Begin();
            return uow;
        }
        
        public InterruptableEfUnitOfWork<TestDbContext> BeginInterruptableUnitOfWork(IClock clock = null, IIdentity identity = null)
        {
            ICurrentTHolder<IIdentity> currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(identity ?? new SystemIdentity());
            var uow = new InterruptableEfUnitOfWork<TestDbContext>(clock ?? new FrozenClock(),
                currentIdentityHolder,
                A.Fake<IDomainEventAggregator>(),
                A.Fake<IEventBusScope>(),
                connection => new TestDbContext(OptionsBuilder.Options),
                Connection);
            uow.Begin();
            return uow;
        }
        
        public void Dispose()
        {
            Connection?.Close();
        }
    }
}