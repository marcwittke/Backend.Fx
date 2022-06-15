using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.EfCore5Persistence.Bootstrapping;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.IdGeneration;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Backend.Fx.EfCore5Persistence.Tests
{
    public class TheDbApplicationWithEfCore
    {
        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveIdGenerator(CompositionRootType compositionRootType)
        {
            var sut = new EfCoreTestApplication(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();
            var entityIdGenerator = sut.CompositionRoot.ServiceProvider.GetRequiredService<IEntityIdGenerator>();
            Assert.StrictEqual(sut.EntityIdGenerator, entityIdGenerator);
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveDbConnection(CompositionRootType compositionRootType)
        {
            var sut = new EfCoreTestApplication(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var dbConnection = sp.GetRequiredService<IDbConnection>();
                    Assert.IsType<SqliteConnection>(dbConnection);
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveICanFlush(CompositionRootType compositionRootType)
        {
            var sut = new EfCoreTestApplication(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var canFlush = sp.GetRequiredService<ICanFlush>();
                    Assert.IsType<EfFlush>(canFlush);
                    canFlush.Flush();
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveDbContext(CompositionRootType compositionRootType)
        {
            var sut = new EfCoreTestApplication(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var dbContext = sp.GetRequiredService<DbContext>();
                    Assert.IsType<EfCoreTestDbContext>(dbContext);
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveAggregateMapping(CompositionRootType compositionRootType)
        {
            var sut = new EfCoreTestApplication(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var aggregateMapping = sp.GetRequiredService<IAggregateMapping<Order>>();
                    Assert.IsType<OrderMapping>(aggregateMapping);
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveEfRepositoryForAggregateRoot(CompositionRootType compositionRootType)
        {
            var sut = new EfCoreTestApplication(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Order>>();
                    Assert.IsType<EfRepository<Order>>(repo);
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task AutoCommitsChangesOnCompletingInvocation(CompositionRootType compositionRootType)
        {
            var sut = new EfCoreTestApplication(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Order>>();
                    repo.Add(new Order(sut.EntityIdGenerator.NextId(), "me1"));
                    repo.Add(new Order(sut.EntityIdGenerator.NextId(), "me2"));
                    repo.Add(new Order(sut.EntityIdGenerator.NextId(), "me3"));
                    repo.Add(new Order(sut.EntityIdGenerator.NextId(), "me4"));
                },
                new SystemIdentity(),
                new TenantId(333));

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Order>>();
                    Assert.Equal(4, repo.GetAll().Length);
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task DoesNotCommitChangesOnException(CompositionRootType compositionRootType)
        {
            var sut = new EfCoreTestApplication(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            Assert.Throws<Exception>(
                () => sut.Invoker.Invoke(
                    sp =>
                    {
                        var repo = sp.GetRequiredService<IRepository<Order>>();
                        repo.Add(new Order(sut.EntityIdGenerator.NextId(), "me1"));
                        repo.Add(new Order(sut.EntityIdGenerator.NextId(), "me2"));
                        repo.Add(new Order(sut.EntityIdGenerator.NextId(), "me3"));
                        repo.Add(new Order(sut.EntityIdGenerator.NextId(), "me4"));
                        throw new Exception("intentionally thrown for a test case");
                    },
                    new SystemIdentity(),
                    new TenantId(333)));

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Order>>();
                    Assert.Empty(repo.GetAll());
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task DoesFlushBeforeRaisingDomainEvents(CompositionRootType compositionRootType)
        {
            var sut = new EfCoreTestApplication(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Order>>();
                    var order = new Order(sut.EntityIdGenerator.NextId(), "domain event test");
                    repo.Add(order);
                    sp.GetRequiredService<IDomainEventAggregator>().PublishDomainEvent(new OrderCreated(order.Id));
                    OrderCreatedHandler.ExpectedInvocationCount++;
                },
                new SystemIdentity(),
                new TenantId(333));

            Assert.Equal(OrderCreatedHandler.ExpectedInvocationCount, OrderCreatedHandler.InvocationCount);
        }


        private class EfCoreTestApplication : PersistentApplication
        {
            private int _nextId = 1;
            private static readonly IDatabaseBootstrapper DbBootstrapper = A.Fake<IDatabaseBootstrapper>();
            public readonly IEntityIdGenerator EntityIdGenerator = A.Fake<IEntityIdGenerator>();

            public EfCoreTestApplication(CompositionRootType compositionRootType, string connectionString)
                : base(
                    DbBootstrapper,
                    A.Fake<IDatabaseAvailabilityAwaiter>(),
                    new BackendFxApplication(
                        compositionRootType.Create(),
                        A.Fake<IExceptionLogger>(),
                        typeof(TheDbApplicationWithEfCore).Assembly))
            {
                A.CallTo(() => DbBootstrapper.EnsureDatabaseExistence()).Invokes(() =>
                {
                    var dbContext = new EfCoreTestDbContext(new DbContextOptionsBuilder<EfCoreTestDbContext>()
                        .UseSqlite(connectionString).Options);
                    dbContext.Database.EnsureCreated();
                });

                var dbConnectionFactory = A.Fake<IDbConnectionFactory>();
                A.CallTo(() => dbConnectionFactory.Create()).Returns(new SqliteConnection(connectionString));

                A.CallTo(() => EntityIdGenerator.NextId())
                    .ReturnsLazily(() => _nextId++);

                var loggerFactory = A.Fake<ILoggerFactory>();

                CompositionRoot.RegisterModules(
                    new EfCorePersistenceModule<EfCoreTestDbContext>(
                        dbConnectionFactory,
                        EntityIdGenerator,
                        loggerFactory,
                        (builder, connection) => builder.UseSqlite((DbConnection)connection),
                        Assemblies
                    ));
            }
        }
    }

    public class EfCoreTestDbContext : DbContext
    {
        public EfCoreTestDbContext(DbContextOptions<EfCoreTestDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
    }

    public class Order : AggregateRoot
    {
        public string Recipient { get; private set; }

        public Order(int id, string recipient) : base(id)
        {
            Recipient = recipient;
        }
    }

    public class OrderMapping : PlainAggregateMapping<Order>
    {
    }

    public class OrderAuthorization : AllowAll<Order>
    {
    }

    public class OrderCreated : IDomainEvent
    {
        public int OrderId { get; }

        public OrderCreated(int orderId)
        {
            OrderId = orderId;
        }
    }

    public class OrderCreatedHandler : IDomainEventHandler<OrderCreated>
    {
        private readonly DbContext _dbContext;
        public static int InvocationCount = 0;
        public static int ExpectedInvocationCount = 0;

        public OrderCreatedHandler(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Handle(OrderCreated domainEvent)
        {
            InvocationCount++;
            Assert.NotNull(_dbContext.Set<Order>().Find(domainEvent.OrderId));
        }
    }
}