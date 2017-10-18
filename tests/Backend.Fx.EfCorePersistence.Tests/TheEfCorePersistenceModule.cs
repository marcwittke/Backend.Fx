namespace Backend.Fx.EfCorePersistence.Tests
{
    using System.Linq;
    using System.Reflection;
    using Bootstrapping;
    using Bootstrapping.Modules;
    using BuildingBlocks;
    using DummyImpl;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Patterns.UnitOfWork;
    using Xunit;

    public class TheEfCorePersistenceModule
    {
        private readonly SimpleInjectorCompositionRoot sut;
        
        public TheEfCorePersistenceModule()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;

            sut = new SimpleInjectorCompositionRoot();
            sut.RegisterModules(
                new DomainModule(sut, typeof(Blog).GetTypeInfo().Assembly),
                new EfCorePersistenceModule<TestDbContext, DbContextOptions<TestDbContext>>(sut, dbContextOptions),
                new ClockModule<FrozenClock>(sut));
            sut.Verify();
        }

        [Fact]
        public void CanResolveRepository()
        {
            using (sut.BeginScope(new SystemIdentity(), new TenantId(111)))
            {
                var repo = sut.GetInstance<IRepository<Blog>>();
                Assert.NotNull(repo);
                Assert.IsType<EfRepository<Blog>>(repo);
            }
        }

        [Fact]
        public void CanResolveDbContext()
        {
            using (sut.BeginScope(new SystemIdentity(), new TenantId(111)))
            {
                var dbContext = sut.GetInstance<DbContext>();
                Assert.NotNull(dbContext);
                Assert.IsType<TestDbContext>(dbContext);
            }
        }

        [Fact]
        public void CanResolveTestDbContext()
        {
            using (sut.BeginScope(new SystemIdentity(), new TenantId(111)))
            {
                var dbContext = sut.GetInstance<TestDbContext>();
                Assert.NotNull(dbContext);
                Assert.IsType<TestDbContext>(dbContext);
            }
        }

        [Fact]
        public void CanResolveAggregateQueryable()
        {
            using (sut.BeginScope(new SystemIdentity(), new TenantId(111)))
            {
                var queryable = sut.GetInstance<IQueryable<Blog>>();
                Assert.NotNull(queryable);
                Assert.IsType<AggregateQueryable<Blog>>(queryable);
            }
        }

        [Fact]
        public void CanBeginEfUnitOfWork()
        {
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(111)))
            {
                using (var unitOfWork = scope.BeginUnitOfWork(false))
                {
                    Assert.NotNull(unitOfWork);
                    Assert.IsType<EfUnitOfWork>(unitOfWork);
                }
            }
        }

        [Fact]
        public void CanBeginReadonlyEfUnitOfWork()
        {
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(111)))
            {
                using (var unitOfWork = scope.BeginUnitOfWork(true))
                {
                    Assert.NotNull(unitOfWork);
                    Assert.IsType<ReadonlyEfUnitOfWork>(unitOfWork);
                }
            }
        }

        [Fact]
        public void CanResolveTransactionInterruptor()
        {
            using (var scope = sut.BeginScope(new SystemIdentity(), new TenantId(100)))
            {
                using (var unitOfWork = scope.BeginUnitOfWork(false))
                {
                    ICanInterruptTransaction canInterruptTransaction = scope.GetInstance<ICanInterruptTransaction>();
                    Assert.Same(unitOfWork, canInterruptTransaction);
                }
            }
        }
    }
}
