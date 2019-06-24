using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
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
    public class TheEfUnitOfWork
    {
        private readonly DatabaseFixture _fixture;
        
        public TheEfUnitOfWork()
        {
            _fixture = new SqliteDatabaseFixture();
            //_fixture = new SqlServerDatabaseFixture();
            _fixture.CreateDatabase();
        }

        [Fact]
        public void OpensTransaction()
        {
            using(var dbs = _fixture.UseDbSession())
            {
                using (var sut = new EfUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), 
                    A.Fake<IDomainEventAggregator>(), A.Fake<IEventBusScope>(), dbs.DbContext, dbs.Connection))
                {
                    Assert.Null(dbs.DbContext.Database.CurrentTransaction);
                    sut.Begin();
                    Assert.NotNull(dbs.DbContext.Database.CurrentTransaction);
                    Assert.Equal(dbs.Connection, dbs.DbContext.Database.CurrentTransaction.GetDbTransaction().Connection);

                    dbs.DbContext.Add(new Blogger(333, "Metulsky", "Bratislav"));
                    sut.Complete();
                }

                Assert.Throws<InvalidOperationException>(()=> dbs.DbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbs = _fixture.UseDbSession())
            {
                Assert.NotNull(dbs.DbContext.Bloggers.SingleOrDefault(b => b.Id == 333 && b.FirstName == "Bratislav" && b.LastName == "Metulsky"));
            }
        }

        [Fact]
        public void RollsBackTransactionOnDisposal()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                using (var sut = new EfUnitOfWork(new FrozenClock(), 
                    CurrentIdentityHolder.CreateSystem(), 
                    A.Fake<IDomainEventAggregator>(), 
                    A.Fake<IEventBusScope>(),
                    dbs.DbContext,
                    dbs.Connection))
                {
                    sut.Begin();
                    dbs.DbContext.Add(new Blogger(333, "Metulsky", "Bratislav"));
                    dbs.DbContext.SaveChanges();
                }

                Assert.Throws<InvalidOperationException>(() => dbs.DbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbs = _fixture.UseDbSession())
            {
                Assert.Null(dbs.DbContext.Bloggers.SingleOrDefault(b => b.Id == 333 && b.FirstName == "Bratislav" && b.LastName == "Metulsky"));
            }
        }

        [Fact]
        public void CanInterruptTransaction()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                using (var sut = new EfUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(),
                    A.Fake<IDomainEventAggregator>(), A.Fake<IEventBusScope>(), dbs.DbContext, dbs.Connection))
                {
                    sut.Begin();
                    dbs.DbContext.Add(new Blogger(333, "Metulsky", "Bratislav"));
                    sut.CompleteCurrentTransaction_BeginNewTransaction();
                    dbs.DbContext.Add(new Blogger(334, "Flash", "Johnny"));
                    sut.Complete();
                }

                Assert.Throws<InvalidOperationException>(() => dbs.DbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbs = _fixture.UseDbSession())
            {
                Assert.NotNull(dbs.DbContext.Bloggers.SingleOrDefault(b => b.Id == 333 && b.FirstName == "Bratislav" && b.LastName == "Metulsky"));
                Assert.NotNull(dbs.DbContext.Bloggers.SingleOrDefault(b => b.Id == 334 && b.FirstName == "Johnny" && b.LastName == "Flash"));
            }
        }

        [Fact]
        public void ClearingTransactionOnRelationalConnectionViaReflectionWorks()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                using (dbs.Connection.OpenDisposable())
                {
                    using (var tx = dbs.Connection.BeginTransaction())
                    {
                        dbs.DbContext.Database.UseTransaction((DbTransaction) tx);
                        dbs.DbContext.Bloggers.Add(new Blogger(1, "bbb", "fff"));
                        dbs.DbContext.SaveChanges();
                        tx.Commit();
                    }



                    // see EfUnitOfWork.cs ClearTransactions()
                    RelationalConnection txman = (RelationalConnection) dbs.DbContext.Database.GetService<IDbContextTransactionManager>();
                    var methodInfo = typeof(RelationalConnection).GetMethod("ClearTransactions", BindingFlags.Instance | BindingFlags.NonPublic);
                    methodInfo.Invoke(txman, new object[0]);

                    using (var tx = dbs.Connection.BeginTransaction())
                    {
                        dbs.DbContext.Database.UseTransaction((DbTransaction) tx);
                        dbs.DbContext.Bloggers.Add(new Blogger(2, "bbb", "fff"));
                        dbs.DbContext.SaveChanges();
                        tx.Commit();
                    }
                }
            }
        }
    }
}
