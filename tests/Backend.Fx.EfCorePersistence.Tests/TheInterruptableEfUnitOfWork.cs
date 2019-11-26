using System;
using System.Linq;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TheInterrubtableEfUnitOfWork
    {
        private readonly DatabaseFixture _fixture;
        
        public TheInterrubtableEfUnitOfWork()
        {
            _fixture = new SqliteDatabaseFixture();
            //_fixture = new SqlServerDatabaseFixture();
            _fixture.CreateDatabase();
        }

        
        [Fact]
        public void CanInterruptTransaction()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                using (InterruptableEfUnitOfWork<TestDbContext> sut = dbs.BeginInterruptableUnitOfWork())
                {
                    sut.DbContext.Add(new Blogger(333, "Metulsky", "Bratislav"));
                    sut.CompleteCurrentTransaction_BeginNewTransaction();
                    sut.DbContext.Add(new Blogger(334, "Flash", "Johnny"));
                    sut.Complete();
                    Assert.Throws<InvalidOperationException>(() => sut.DbContext.Database.CurrentTransaction.Commit()); 
                }
            }

            using (DbSession dbs = _fixture.UseDbSession())
            {
                using (EfUnitOfWork<TestDbContext> sut = dbs.BeginUnitOfWork())
                {
                    Assert.NotNull(sut.DbContext.Bloggers.SingleOrDefault(b => b.Id == 333 && b.FirstName == "Bratislav" && b.LastName == "Metulsky"));
                    Assert.NotNull(sut.DbContext.Bloggers.SingleOrDefault(b => b.Id == 334 && b.FirstName == "Johnny" && b.LastName == "Flash"));
                }
            }
        }

    }
}
