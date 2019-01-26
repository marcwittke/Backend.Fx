using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    //public class TheEfUnitOfWork : TestWithSqlServerDbContext
    public class TheEfUnitOfWork : TestWithInMemorySqliteDbContext
    {
        public TheEfUnitOfWork()
        {
            CreateDatabase();
        }

        [Fact]
        public void OpensTransaction()
        {
            using(var dbContext = UseDbContext())
            {
                using (var sut = new EfUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), 
                    A.Fake<IDomainEventAggregator>(), A.Fake<IEventBusScope>(), dbContext, Connection))
                {
                    Assert.Null(dbContext.Database.CurrentTransaction);
                    sut.Begin();
                    Assert.NotNull(dbContext.Database.CurrentTransaction);
                    Assert.Equal(Connection, dbContext.Database.CurrentTransaction.GetDbTransaction().Connection);

                    dbContext.Add(new Blogger(333, "Metulsky", "Bratislav"));
                    sut.Complete();
                }

                Assert.Throws<InvalidOperationException>(()=> dbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbContext = UseDbContext())
            {
                Assert.NotNull(dbContext.Bloggers.SingleOrDefault(b => b.Id == 333 && b.FirstName == "Bratislav" && b.LastName == "Metulsky"));
            }
        }

        [Fact]
        public void RollsBackTransactionOnDisposal()
        {
            using (var dbContext = UseDbContext())
            {
                using (var sut = new EfUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), A.Fake<IDomainEventAggregator>(), A.Fake<IEventBusScope>(), dbContext, Connection))
                {
                    sut.Begin();
                    dbContext.Add(new Blogger(333, "Metulsky", "Bratislav"));
                    dbContext.SaveChanges();
                }

                Assert.Throws<InvalidOperationException>(() => dbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbContext = UseDbContext())
            {
                Assert.Null(dbContext.Bloggers.SingleOrDefault(b => b.Id == 333 && b.FirstName == "Bratislav" && b.LastName == "Metulsky"));
            }
        }

        [Fact]
        public void CanInterruptTransaction()
        {
            using (var dbContext = UseDbContext())
            {
                using (var sut = new EfUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(),
                    A.Fake<IDomainEventAggregator>(), A.Fake<IEventBusScope>(), dbContext, Connection))
                {
                    sut.Begin();
                    dbContext.Add(new Blogger(333, "Metulsky", "Bratislav"));
                    sut.CompleteCurrentTransaction_BeginNewTransaction();
                    dbContext.Add(new Blogger(334, "Flash", "Johnny"));
                    sut.Complete();
                }

                Assert.Throws<InvalidOperationException>(() => dbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbContext = UseDbContext())
            {
                Assert.NotNull(dbContext.Bloggers.SingleOrDefault(b => b.Id == 333 && b.FirstName == "Bratislav" && b.LastName == "Metulsky"));
                Assert.NotNull(dbContext.Bloggers.SingleOrDefault(b => b.Id == 334 && b.FirstName == "Johnny" && b.LastName == "Flash"));
            }
        }

        [Fact]
        public void ClearingTransactionOnRelationalConnectionViaReflectionWorks()
        {
            var connection = Connection;
            
            using (var dbContext = UseDbContext())
            {
                
                using (var tx = connection.BeginTransaction())
                {
                    dbContext.Database.UseTransaction((DbTransaction) tx);
                    dbContext.Bloggers.Add(new Blogger(1, "bbb", "fff"));
                    dbContext.SaveChanges();
                    tx.Commit();
                    dbContext.Database.CloseConnection();
                }

                // see EfUnitOfWork.cs ClearTransactions()
                RelationalConnection txman = (RelationalConnection)dbContext.Database.GetService<IDbContextTransactionManager>();
                var methodInfo = typeof(RelationalConnection).GetMethod("ClearTransactions", BindingFlags.Instance | BindingFlags.NonPublic);
                methodInfo.Invoke(txman, new object[0]);

                using (var tx = connection.BeginTransaction())
                {
                    dbContext.Database.UseTransaction((DbTransaction) tx);
                    dbContext.Bloggers.Add(new Blogger(2, "bbb", "fff"));
                    dbContext.SaveChanges();
                    tx.Commit();
                }
            }
        }
    }
}
