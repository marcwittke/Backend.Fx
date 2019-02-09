using System;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
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
                var sut = new ReadonlyEfUnitOfWork(dbs.Connection, dbs.DbContext, CurrentIdentityHolder.CreateSystem());
                
                Assert.Null(dbs.DbContext.Database.CurrentTransaction);
                sut.Begin();
                Assert.NotNull(dbs.DbContext.Database.CurrentTransaction);
                sut.Complete();
                Assert.Throws<InvalidOperationException>(() => dbs.DbContext.Database.CurrentTransaction.Commit());
            }
        }

        [Fact]
        public void RollbacksTransactionOnComplete()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                var sut = new ReadonlyEfUnitOfWork(dbs.Connection, dbs.DbContext, CurrentIdentityHolder.CreateSystem());
                sut.Begin();
                dbs.DbContext.Add(new Blogger(334, "Bratislav", "Metulsky"));
                sut.Complete();
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
                var sut = new ReadonlyEfUnitOfWork(dbs.Connection, dbs.DbContext, CurrentIdentityHolder.CreateSystem());
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
