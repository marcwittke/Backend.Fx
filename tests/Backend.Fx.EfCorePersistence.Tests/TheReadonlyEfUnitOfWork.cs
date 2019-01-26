using System;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    using Environment.Authentication;

    public class TheReadonlyEfUnitOfWork : TestWithInMemorySqliteDbContext
    {
        public TheReadonlyEfUnitOfWork()
        {
            CreateDatabase();
        }

        [Fact]
        public void OpensTransaction()
        {
            using(var dbContext = UseDbContext())
            {
                var sut = new ReadonlyEfUnitOfWork(Connection, dbContext, CurrentIdentityHolder.CreateSystem());
                
                Assert.Null(dbContext.Database.CurrentTransaction);
                sut.Begin();
                Assert.NotNull(dbContext.Database.CurrentTransaction);
                sut.Complete();
                Assert.Throws<InvalidOperationException>(() => dbContext.Database.CurrentTransaction.Commit());
            }
        }

        [Fact]
        public void RollbacksTransactionOnComplete()
        {
            using (var dbContext = UseDbContext())
            {
                var sut = new ReadonlyEfUnitOfWork(Connection, dbContext, CurrentIdentityHolder.CreateSystem());
                sut.Begin();
                dbContext.Add(new Blogger(334, "Bratislav", "Metulsky"));
                sut.Complete();
                Assert.Throws<InvalidOperationException>(() => dbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbContext = UseDbContext())
            {
                Assert.Empty(dbContext.Bloggers);
            }
        }

        [Fact]
        public void RollbacksTransactionOnDisposal()
        {
            using (var dbContext = UseDbContext())
            {
                var sut = new ReadonlyEfUnitOfWork(Connection, dbContext, CurrentIdentityHolder.CreateSystem());
                sut.Begin();
                dbContext.Add(new Blogger(335, "Bratislav", "Metulsky"));
                sut.Dispose();
                Assert.Throws<InvalidOperationException>(() => dbContext.Database.CurrentTransaction.Commit());
            }

            using (var dbContext = UseDbContext())
            {
                Assert.Empty(dbContext.Bloggers);
            }
        }
    }
}
