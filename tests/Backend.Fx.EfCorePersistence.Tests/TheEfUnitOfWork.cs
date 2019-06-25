using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.Authorization;
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
        private static int _nextTenantId = 2675;
        private readonly int _tenantId = _nextTenantId++;

        public TheEfUnitOfWork()
        {
            _fixture = new SqliteDatabaseFixture();
            //_fixture = new SqlServerDatabaseFixture();
            _fixture.CreateDatabase();
        }

        [Fact]
        public void OpensTransaction()
        {
            using (var dbs = _fixture.UseDbSession())
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

                Assert.Throws<InvalidOperationException>(() => dbs.DbContext.Database.CurrentTransaction.Commit());
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
                        dbs.DbContext.Database.UseTransaction((DbTransaction)tx);
                        dbs.DbContext.Bloggers.Add(new Blogger(1, "bbb", "fff"));
                        dbs.DbContext.SaveChanges();
                        tx.Commit();
                    }



                    // see EfUnitOfWork.cs ClearTransactions()
                    RelationalConnection txman = (RelationalConnection)dbs.DbContext.Database.GetService<IDbContextTransactionManager>();
                    var methodInfo = typeof(RelationalConnection).GetMethod("ClearTransactions", BindingFlags.Instance | BindingFlags.NonPublic);
                    methodInfo.Invoke(txman, new object[0]);

                    using (var tx = dbs.Connection.BeginTransaction())
                    {
                        dbs.DbContext.Database.UseTransaction((DbTransaction)tx);
                        dbs.DbContext.Bloggers.Add(new Blogger(2, "bbb", "fff"));
                        dbs.DbContext.SaveChanges();
                        tx.Commit();
                    }
                }
            }
        }

        public class AnEvent : IDomainEvent { }

        public class AnEventHandler : IDomainEventHandler<AnEvent>
        {
            private readonly IRepository<Blog> _blogRepository;

            public AnEventHandler(IRepository<Blog> blogRepository)
            {
                _blogRepository = blogRepository;
            }

            public void Handle(AnEvent domainEvent)
            {
                _blogRepository.Add(new Blog(99999, "Created via Event Handling"));
            }
        }

        [Fact]
        public void FlushesAfterDomainEventHandling()
        {
            IDomainEventHandlerProvider fakeEventHandlerProvider = A.Fake<IDomainEventHandlerProvider>();
            var domainEventAggregator = new DomainEventAggregator(fakeEventHandlerProvider);


            using (var dbs = _fixture.UseDbSession())
            {
                A.CallTo(
                        () => fakeEventHandlerProvider.GetAllEventHandlers<AnEvent>())
                    .ReturnsLazily(() =>
                    {
                        var repo = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(),
                            CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                        return new[] {new AnEventHandler(repo)};
                    });

                using (var sut = new EfUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(),
                    domainEventAggregator, A.Fake<IEventBusScope>(), dbs.DbContext, dbs.Connection))
                {
                    sut.Begin();
                    domainEventAggregator.PublishDomainEvent(new AnEvent());
                    sut.Complete();
                }
            }

            using (var dbs = _fixture.UseDbSession())
            {
                var createdViaEvent = dbs.DbContext.Blogs.Find(99999);
                Assert.NotNull(createdViaEvent);
                Assert.Equal("Created via Event Handling", createdViaEvent.Name);
            }
        }
    }
}
