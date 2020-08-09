using System;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
using Backend.Fx.Patterns.UnitOfWork;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TheReadonlyTransactionContext
    {
        public TheReadonlyTransactionContext()
        {
            _fixture = new SqliteDatabaseFixture();
            //_fixture = new SqlServerDatabaseFixture();
            _fixture.CreateDatabase();
        }

        private readonly DatabaseFixture _fixture;

        [Fact]
        public void OpensTransaction()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                IUnitOfWork sut = dbs.BeginUnitOfWork(true);
                Assert.NotNull(sut.GetDbContext());
                Assert.NotNull(sut.GetDbContext().Database.CurrentTransaction);
                sut.Complete();
                Assert.Throws<InvalidOperationException>(() => sut.GetDbContext().Database.CurrentTransaction.Commit());
            }
        }

        [Fact]
        public void RollbacksTransactionOnComplete()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                IUnitOfWork sut = dbs.BeginUnitOfWork(true);
                sut.GetDbContext().Add(new Blogger(334, "Bratislav", "Metulsky"));
                sut.Complete();
                Assert.Throws<InvalidOperationException>(() => sut.GetDbContext().Database.CurrentTransaction.Commit());
            }

            using (DbSession dbs = _fixture.UseDbSession())
            {
                IUnitOfWork sut = dbs.BeginUnitOfWork();
                Assert.Empty(sut.GetDbContext().Set<Blogger>());
            }
        }

        [Fact]
        public void RollbacksTransactionOnDisposal()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                IUnitOfWork sut = dbs.BeginUnitOfWork(true);
                sut.GetDbContext().Add(new Blogger(335, "Bratislav", "Metulsky"));
                sut.GetDbTransaction().Dispose();
                Assert.Throws<InvalidOperationException>(() => sut.GetDbContext().Database.CurrentTransaction.Commit());
            }

            using (DbSession dbs = _fixture.UseDbSession())
            {
                IUnitOfWork sut = dbs.BeginUnitOfWork();
                Assert.Empty(sut.GetDbContext().Set<Blogger>());
            }
        }
    }
}