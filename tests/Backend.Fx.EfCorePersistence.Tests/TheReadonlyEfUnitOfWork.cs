using System;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
using Backend.Fx.Patterns.UnitOfWork;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
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
            using(DbSession dbs = _fixture.UseDbSession())
            {
                IUnitOfWork unitOfWork = dbs.BeginUnitOfWork();
                var sut = new ReadonlyDecorator(unitOfWork);
                Assert.NotNull(unitOfWork.GetDbContext());
                Assert.NotNull(unitOfWork.GetDbContext().Database.CurrentTransaction);
                sut.Complete();
                sut.Dispose();
                Assert.Throws<InvalidOperationException>(() => unitOfWork.GetDbContext().Database.CurrentTransaction.Commit());
            }
        }

        [Fact]
        public void RollbacksTransactionOnComplete()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                IUnitOfWork unitOfWork = dbs.BeginUnitOfWork();
                var sut = new ReadonlyDecorator(unitOfWork);
                unitOfWork.GetDbContext().Add(new Blogger(334, "Bratislav", "Metulsky"));
                sut.Complete();
                sut.Dispose();
                Assert.Throws<InvalidOperationException>(() => unitOfWork.GetDbContext().Database.CurrentTransaction.Commit());
            }

            using (DbSession dbs = _fixture.UseDbSession())
            {
                using (IUnitOfWork sut = dbs.BeginUnitOfWork())
                {
                    Assert.Empty(sut.GetDbContext().Set<Blogger>());
                }
            }
        }

        [Fact]
        public void RollbacksTransactionOnDisposal()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                IUnitOfWork unitOfWork = dbs.BeginUnitOfWork();
                var sut = new ReadonlyDecorator(unitOfWork);
                unitOfWork.GetDbContext().Add(new Blogger(335, "Bratislav", "Metulsky"));
                sut.Dispose();
                Assert.Throws<InvalidOperationException>(() => unitOfWork.GetDbContext().Database.CurrentTransaction.Commit());
            }

            using (DbSession dbs = _fixture.UseDbSession())
            {
                using (IUnitOfWork sut = dbs.BeginUnitOfWork())
                {
                    Assert.Empty(sut.GetDbContext().Set<Blogger>());
                }
            }
        }
    }
}
