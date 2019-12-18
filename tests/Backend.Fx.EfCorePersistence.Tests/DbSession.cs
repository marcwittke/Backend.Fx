using System;
using System.Data.Common;
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

        public IUnitOfWork BeginUnitOfWork(IClock clock = null, IIdentity identity = null)
        {
            ICurrentTHolder<IIdentity> currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(identity ?? new SystemIdentity());
            var efuow = new EfUnitOfWork<TestDbContext>(clock ?? new FrozenClock(),
                currentIdentityHolder,
                A.Fake<IDomainEventAggregator>(),
                A.Fake<IEventBusScope>(),
                new TestDbContext(OptionsBuilder.Options));
            var uow = new EfUnitOfWorkTransactionDecorator<TestDbContext>(Connection, efuow);
            uow.Begin();
            return uow;
        }
        
        public void Dispose()
        {
            Connection?.Close();
        }
    }

    public static class TestEx
    {
        public static DbContext GetDbContext(this IUnitOfWork unitOfWork)
        {
            if (unitOfWork is EfUnitOfWork<TestDbContext> efUnitOfWork)
            {
                return efUnitOfWork.DbContext;
            }

            if (unitOfWork is UnitOfWorkTransactionDecorator transactionDecorator)
            {
                return GetDbContext(transactionDecorator.UnitOfWork);
            }

            throw new InvalidOperationException();
        }
        
        public static DbTransaction GetDbTransaction(this IUnitOfWork unitOfWork)
        {
            if (unitOfWork is UnitOfWorkTransactionDecorator transactionDecorator)
            {
                return transactionDecorator.TransactionContext.CurrentTransaction;
            }

            throw new InvalidOperationException();
        }
    }
}