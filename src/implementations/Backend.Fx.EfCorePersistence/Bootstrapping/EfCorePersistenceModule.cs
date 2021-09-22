using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.IdGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public class EfCorePersistenceModule<TDbContext> : IModule
        where TDbContext : DbContext
    {
        private readonly Assembly[] _assemblies;
        private readonly Action<DbContextOptionsBuilder<TDbContext>, IDbConnection> _configure;
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IEntityIdGenerator _entityIdGenerator;
        private readonly ILoggerFactory _loggerFactory;

        public EfCorePersistenceModule(
            IDbConnectionFactory dbConnectionFactory,
            IEntityIdGenerator entityIdGenerator,
            ILoggerFactory loggerFactory,
            Action<DbContextOptionsBuilder<TDbContext>, IDbConnection> configure,
            params Assembly[] assemblies)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _entityIdGenerator = entityIdGenerator;
            _loggerFactory = loggerFactory;
            _configure = configure;
            _assemblies = assemblies;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            // by letting the container create the connection we can be sure, that only one connection per scope is used, and disposing is done accordingly
            compositionRoot.InfrastructureModule.RegisterScoped(() => _dbConnectionFactory.Create());

            // singleton id generator
            compositionRoot.InfrastructureModule.RegisterInstance(_entityIdGenerator);

            // EF core requires us to flush frequently, because of a missing identity map
            compositionRoot.InfrastructureModule.RegisterScoped<ICanFlush, EfFlush>();

            // EF Repositories
            compositionRoot.InfrastructureModule.RegisterScoped(typeof(IRepository<>), typeof(EfRepository<>));

            // IQueryable is supported, but should be use with caution, since it bypasses authorization
            compositionRoot.InfrastructureModule.RegisterScoped(typeof(IQueryable<>), typeof(EntityQueryable<>));

            // DbContext is injected into repositories
            compositionRoot.InfrastructureModule.RegisterScoped(
                () => CreateDbContextOptions(compositionRoot.InstanceProvider.GetInstance<IDbConnection>()));
            compositionRoot.InfrastructureModule.RegisterScoped<DbContext, TDbContext>();

            // wrapping the operation: connection.open - transaction.begin - operation - (flush) - transaction.commit - connection.close
            compositionRoot.InfrastructureModule.RegisterDecorator<IOperation, FlushOperationDecorator>();
            compositionRoot.InfrastructureModule
                .RegisterDecorator<IOperation, DbContextTransactionOperationDecorator>();
            compositionRoot.InfrastructureModule.RegisterDecorator<IOperation, DbConnectionOperationDecorator>();

            // ensure everything dirty is flushed to the db before handling domain events  
            compositionRoot.InfrastructureModule
                .RegisterDecorator<IDomainEventAggregator, FlushDomainEventAggregatorDecorator>();

            compositionRoot.InfrastructureModule.RegisterScoped(typeof(IAggregateMapping<>), _assemblies);
        }

        protected virtual DbContextOptions<TDbContext> CreateDbContextOptions(IDbConnection connection)
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            _configure.Invoke(dbContextOptionsBuilder, connection);
            return dbContextOptionsBuilder.UseLoggerFactory(_loggerFactory).Options;
        }
    }
}
