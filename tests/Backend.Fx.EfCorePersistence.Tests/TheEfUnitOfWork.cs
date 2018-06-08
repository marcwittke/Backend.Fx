namespace Backend.Fx.EfCorePersistence.Tests
{
    using DummyImpl;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using FakeItEasy;
    using Patterns.EventAggregation.Domain;
    using Xunit;

    public class TheEfUnitOfWork : TestWithInMemorySqliteDbContext
    {
    
        public TheEfUnitOfWork()
        {
            CreateDatabase();
        }

        [Fact]
        public void OpensTransaction()
        {
            using(var dbContext = new TestDbContext(DbContextOptions))
            {
                var sut = new EfUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), A.Fake<IDomainEventAggregator>(), dbContext);
                
                Assert.Null(dbContext.Database.CurrentTransaction);
                sut.Begin();
                Assert.NotNull(dbContext.Database.CurrentTransaction);
                sut.Complete();
                Assert.Null(dbContext.Database.CurrentTransaction);
            }
        }

        [Fact]
        public void RollbacksTransactionOnDisposal()
        {
            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                var sut = new EfUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), A.Fake<IDomainEventAggregator>(), dbContext);
                sut.Begin();
                dbContext.Add(new Blogger(333, "Bratislav", "Metulsky"));
                sut.Dispose();
                Assert.Null(dbContext.Database.CurrentTransaction);
                Assert.Empty(dbContext.Bloggers);
            }
        }
    }
}
