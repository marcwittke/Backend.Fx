using System;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    using Environment.Authentication;

    public class TheReadonlyEfUnitOfWork
    {
        private readonly DatabaseFixture _fixture;

        public TheReadonlyEfUnitOfWork()
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
                var sut = new ReadonlyDecorator(new EfUnitOfWork(
                                                    new FrozenClock(),
                                                    CurrentIdentityHolder.CreateSystem(),
                                                    A.Fake<IDomainEventAggregator>(),
                                                    A.Fake<IEventBusScope>(),
                                                    dbs.DbContext,
                                                    dbs.Connection));
                
                Assert.Null(dbs.DbContext.Database.CurrentTransaction);
                sut.Begin();
                Assert.NotNull(dbs.DbContext.Database.CurrentTransaction);
                sut.Complete();
                sut.Dispose();
                Assert.Throws<InvalidOperationException>(() => dbs.DbContext.Database.CurrentTransaction.Commit());
            }
        }

        [Fact]
        public void RollbacksTransactionOnComplete()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                var sut = new ReadonlyDecorator(new EfUnitOfWork(
                                                    new FrozenClock(),
                                                    CurrentIdentityHolder.CreateSystem(),
                                                    A.Fake<IDomainEventAggregator>(),
                                                    A.Fake<IEventBusScope>(),
                                                    dbs.DbContext,
                                                    dbs.Connection));
                sut.Begin();
                dbs.DbContext.Add(new Blogger(334, "Bratislav", "Metulsky"));
                sut.Complete();
                sut.Dispose();
                Assert.Throws<InvalidOperationException>(() => dbs.DbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbs = _fixture.UseDbSession())
            {
                Assert.Empty(dbs.DbContext.Bloggers);
            }
        }

        [Fact]
        public void RollbacksTransactionOnDisposal()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                var sut = new ReadonlyDecorator(new EfUnitOfWork(
                                                    new FrozenClock(),
                                                    CurrentIdentityHolder.CreateSystem(),
                                                    A.Fake<IDomainEventAggregator>(),
                                                    A.Fake<IEventBusScope>(),
                                                    dbs.DbContext,
                                                    dbs.Connection));
                sut.Begin();
                dbs.DbContext.Add(new Blogger(335, "Bratislav", "Metulsky"));
                sut.Dispose();
                Assert.Throws<InvalidOperationException>(() => dbs.DbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbs = _fixture.UseDbSession())
            {
                Assert.Empty(dbs.DbContext.Bloggers);
            }
        }
    }
}
