using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Backend.Fx.Domain;
using Backend.Fx.EfCore6Persistence.Tests.SampleApp.Persistence;
using Backend.Fx.EfCore6Persistence.Tests.SampleApp.Runtime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Extensions.Persistence;
using Backend.Fx.Features.DomainEvents;
using Backend.Fx.TestUtil;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleApp.Domain;
using Xunit;

namespace Backend.Fx.EfCore6Persistence.Tests
{
    public class TheDbApplicationWithEfCore
    {
        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveIdGenerator(CompositionRootType compositionRootType)
        {
            var sut = SampleAppBuilder.Build(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();
            var entityIdGenerator = sut.CompositionRoot.ServiceProvider.GetRequiredService<IEntityIdGenerator>();
            Assert.IsType<SampleAppIdGenerator>(entityIdGenerator);
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveDbConnection(CompositionRootType compositionRootType)
        {
            var sut = SampleAppBuilder.Build(compositionRootType, "Data Source=" + Path.GetTempFileName());
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
            var sut = SampleAppBuilder.Build(compositionRootType, "Data Source=" + Path.GetTempFileName());
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
            var sut = SampleAppBuilder.Build(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var dbContext = sp.GetRequiredService<DbContext>();
                    Assert.IsType<SampleAppDbContext>(dbContext);
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveAggregateMapping(CompositionRootType compositionRootType)
        {
            var sut = SampleAppBuilder.Build(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var aggregateMapping = sp.GetRequiredService<IAggregateMapping<Blog>>();
                    Assert.IsType<BlogMapping>(aggregateMapping);
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveEfRepositoryForAggregateRoot(CompositionRootType compositionRootType)
        {
            var sut = SampleAppBuilder.Build(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Blog>>();
                    Assert.IsType<EfRepository<Blog>>(repo);
                },
                new SystemIdentity(),
                new TenantId(333));
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task AutoCommitsChangesOnCompletingInvocation(CompositionRootType compositionRootType)
        {
            var sut = SampleAppBuilder.Build(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Blog>>();
                    var idGen = sp.GetRequiredService<IEntityIdGenerator>();
                    repo.Add(new Blog(idGen.NextId(), "me1"));
                    repo.Add(new Blog(idGen.NextId(), "me2"));
                    repo.Add(new Blog(idGen.NextId(), "me3"));
                    repo.Add(new Blog(idGen.NextId(), "me4"));
                },
                new SystemIdentity(),
                new TenantId(333));

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Blog>>();
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
            var sut = SampleAppBuilder.Build(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            Assert.Throws<Exception>(
                () => sut.Invoker.Invoke(
                    sp =>
                    {
                        var repo = sp.GetRequiredService<IRepository<Blog>>();
                        var idGen = sp.GetRequiredService<IEntityIdGenerator>();
                        repo.Add(new Blog(idGen.NextId(), "me1"));
                        repo.Add(new Blog(idGen.NextId(), "me2"));
                        repo.Add(new Blog(idGen.NextId(), "me3"));
                        repo.Add(new Blog(idGen.NextId(), "me4"));
                        throw new Exception("intentionally thrown for a test case");
                    },
                    new SystemIdentity(),
                    new TenantId(333)));

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Blog>>();
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
            var sut = SampleAppBuilder.Build(compositionRootType, "Data Source=" + Path.GetTempFileName());
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var repo = sp.GetRequiredService<IRepository<Blog>>();
                    var idGen = sp.GetRequiredService<IEntityIdGenerator>();
                    var order = new Blog(idGen.NextId(), "domain event test");
                    repo.Add(order);
                    sp.GetRequiredService<IDomainEventAggregator>().PublishDomainEvent(new BlogCreated(order.Id));
                    BlogCreatedHandler.ExpectedInvocationCount++;
                },
                new SystemIdentity(),
                new TenantId(333));

            Assert.Equal(BlogCreatedHandler.ExpectedInvocationCount, BlogCreatedHandler.InvocationCount);
        }
    }
}