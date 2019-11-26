using System;
using System.Linq;
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
            using (DbSession dbs = _fixture.UseDbSession())
            {
                using (EfUnitOfWork<TestDbContext> sut = dbs.BeginUnitOfWork())
                {
                    Assert.NotNull(sut.DbContext.Database.CurrentTransaction);
                    Assert.Equal(dbs.Connection, sut.DbContext.Database.CurrentTransaction.GetDbTransaction().Connection);

                    sut.DbContext.Add(new Blogger(333, "Metulsky", "Bratislav"));
                    sut.Complete();
                    
                    Assert.Null(sut.CurrentTransaction);
                    Assert.Throws<InvalidOperationException>(() => sut.DbContext.Database.CurrentTransaction.Commit()); // because sut.Complete() did it already
                }
            
                using (EfUnitOfWork<TestDbContext> sut = dbs.BeginUnitOfWork())
                {
                    Assert.NotNull(sut.DbContext.Set<Blogger>().SingleOrDefault(b => b.Id == 333 && b.FirstName == "Bratislav" && b.LastName == "Metulsky"));
                }
            }
        }

        [Fact]
        public void RollsBackTransactionOnDisposal()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                {
                    EfUnitOfWork<TestDbContext> sut = dbs.BeginUnitOfWork();
                    sut.DbContext.Add(new Blogger(333, "Metulsky", "Bratislav"));
                    sut.Flush();
                    // no sut.Complete()
                    sut.Dispose();
                    
                    Assert.Null(sut.CurrentTransaction);
                    Assert.Throws<InvalidOperationException>(() => sut.DbContext.Database.CurrentTransaction.Commit()); // because sut.Dispose() has already rolled back the open tx
                }
                
                using (EfUnitOfWork<TestDbContext> sut = dbs.BeginUnitOfWork())
                {
                    Assert.Null(sut.DbContext.Set<Blogger>().SingleOrDefault(b => b.Id == 333 && b.FirstName == "Bratislav" && b.LastName == "Metulsky"));
                }
            }
        }


        private class AnEvent : IDomainEvent { }

        private class AnEventHandler : IDomainEventHandler<AnEvent>
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
            var fakeEventHandlerProvider = A.Fake<IDomainEventHandlerProvider>();
            var domainEventAggregator = new DomainEventAggregator(fakeEventHandlerProvider);


            using (DbSession dbs = _fixture.UseDbSession())
            {
                using (EfUnitOfWork<TestDbContext> sut = new EfUnitOfWork<TestDbContext>(new FrozenClock(),
                    CurrentIdentityHolder.CreateSystem(),
                    domainEventAggregator,
                    A.Fake<IEventBusScope>(),
                    // ReSharper disable once AccessToDisposedClosure
                    connection => new TestDbContext(dbs.OptionsBuilder.Options),
                    dbs.Connection))
                {
                    sut.Begin();
                    A.CallTo(() => fakeEventHandlerProvider.GetAllEventHandlers<AnEvent>())
                        .ReturnsLazily(() =>
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            var repo = new EfRepository<Blog>(sut.DbContext, new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                            return new[] {new AnEventHandler(repo)};
                        });
                    
                    domainEventAggregator.PublishDomainEvent(new AnEvent());
                    Blog createdViaEvent = sut.DbContext.Set<Blog>().Find(99999);
                    Assert.Null(createdViaEvent);
                    sut.Complete();
                }
            }

            using (DbSession dbs = _fixture.UseDbSession())
            {
                using (EfUnitOfWork<TestDbContext> sut = dbs.BeginUnitOfWork())
                {
                    Blog createdViaEvent = sut.DbContext.Set<Blog>().Find(99999);
                    Assert.NotNull(createdViaEvent);
                    Assert.Equal("Created via Event Handling", createdViaEvent.Name);
                }
            }
        }
    }
}
