namespace Backend.Fx.EfCorePersistence.Tests
{
    using DummyImpl;
    using NLogLogging;
    using Xunit;

    public class TheReadonlyEfUnitOfWork : TestWithInMemorySqliteDbContext, IClassFixture<NLogLoggingFixture>
    {
        public TheReadonlyEfUnitOfWork()
        {
            CreateDatabase();
        }

        [Fact]
        public void OpensTransaction()
        {
            using(var dbContext = new TestDbContext(DbContextOptions))
            {
                var sut = new ReadonlyEfUnitOfWork(dbContext);
                
                Assert.Null(dbContext.Database.CurrentTransaction);
                sut.Begin();
                Assert.NotNull(dbContext.Database.CurrentTransaction);
                sut.Complete();
                Assert.Null(dbContext.Database.CurrentTransaction);
            }
        }

        [Fact]
        public void RollbacksTransactionOnComplete()
        {
            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                var sut = new ReadonlyEfUnitOfWork(dbContext);
                sut.Begin();
                dbContext.Add(new Blogger("Bratislav", "Metulsky"));
                sut.Complete();
                Assert.Null(dbContext.Database.CurrentTransaction);
                Assert.Empty(dbContext.Bloggers);
            }
        }

        [Fact]
        public void RollbacksTransactionOnDisposal()
        {
            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                var sut = new ReadonlyEfUnitOfWork(dbContext);
                sut.Begin();
                dbContext.Add(new Blogger("Bratislav", "Metulsky"));
                sut.Dispose();
                Assert.Null(dbContext.Database.CurrentTransaction);
                Assert.Empty(dbContext.Bloggers);
            }
        }
    }
}
