using System;
using System.Data.Common;
using System.Security.Principal;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.Transactions;
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

        public IUnitOfWork BeginUnitOfWork(bool asReadonly = false, IClock clock = null, IIdentity identity = null)
        {
            ICurrentTHolder<IIdentity> currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(identity ?? new SystemIdentity());

            var dbContext = new TestDbContext(OptionsBuilder.Options);
            IUnitOfWork unitOfWork = new EfUnitOfWork(clock ?? new FrozenClock(),
                currentIdentityHolder,
                A.Fake<IDomainEventAggregator>(),
                A.Fake<IMessageBusScope>(),
                dbContext);

            ITransactionContext transactionContext = new TransactionContext(Connection);
            if (asReadonly)
            {
                transactionContext = new ReadonlyDecorator(transactionContext);
            }
            
            unitOfWork = new DbContextTransactionDecorator(transactionContext,dbContext, unitOfWork);
            // unitOfWork = new DbConnectionDecorator(Connection, unitOfWork);

            unitOfWork.Begin();
            return unitOfWork;
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
            if (unitOfWork is EfUnitOfWork efUnitOfWork)
            {
                return efUnitOfWork.DbContext;
            }

            if (unitOfWork is DbContextTransactionDecorator dbcTransactionDecorator)
            {
                return GetDbContext(dbcTransactionDecorator.UnitOfWork);
            }
            
            if (unitOfWork is DbTransactionDecorator transactionDecorator)
            {
                return GetDbContext(transactionDecorator.UnitOfWork);
            }
            
            if (unitOfWork is DbConnectionDecorator connectionDecorator)
            {
                return GetDbContext(connectionDecorator.UnitOfWork);
            }

            throw new InvalidOperationException();
        }
        
        public static DbTransaction GetDbTransaction(this IUnitOfWork unitOfWork)
        {
            if (unitOfWork is DbContextTransactionDecorator dbcTransactionDecorator)
            {
                return (DbTransaction) dbcTransactionDecorator.TransactionContext.CurrentTransaction;
            }
            
            if (unitOfWork is DbTransactionDecorator transactionDecorator)
            {
                return (DbTransaction) transactionDecorator.TransactionContext.CurrentTransaction;
            }
            
            if (unitOfWork is DbConnectionDecorator connectionDecorator)
            {
                return GetDbTransaction(connectionDecorator.UnitOfWork);
            }

            throw new InvalidOperationException();
        }
    }
}