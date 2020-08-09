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
        private readonly ILoggerFactory _loggerFactory;
        private readonly Action<DbContextOptionsBuilder<TDbContext>, IDbConnection> _configure;
        private readonly IInfrastructureModule _infrastructureModule;
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IEntityIdGenerator _entityIdGenerator;
        private readonly Assembly[] _assemblies;

        public EfCorePersistenceModule(IInfrastructureModule infrastructureModule, IDbConnectionFactory dbConnectionFactory, IEntityIdGenerator entityIdGenerator, 
                                          ILoggerFactory loggerFactory, Action<DbContextOptionsBuilder<TDbContext>, IDbConnection> configure, params Assembly[] assemblies)
        {
            _infrastructureModule = infrastructureModule;
            _dbConnectionFactory = dbConnectionFactory;
            _entityIdGenerator = entityIdGenerator;
            _loggerFactory = loggerFactory;
            _configure = configure;
            _assemblies = assemblies;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            // by letting the container create the connection we can be sure, that only one connection per scope is used, and disposing is done accordingly
            _infrastructureModule.RegisterScoped(() => _dbConnectionFactory.Create());

            // singleton id generator
            _infrastructureModule.RegisterInstance(_entityIdGenerator);

            // EF core requires us to flush frequently, because of a missing identity map
            _infrastructureModule.RegisterScoped<ICanFlush, DbSession>();

            // EF Repositories
            _infrastructureModule.RegisterScoped(typeof(IRepository<>), typeof(EfRepository<>));

            // IQueryable is supported, but should be use with caution, since it bypasses authorization
            _infrastructureModule.RegisterScoped(typeof(IQueryable<>), typeof(EntityQueryable<>));

            // DbContext is injected into repositories
            _infrastructureModule.RegisterScoped(() => CreateDbContextOptions(compositionRoot.InstanceProvider.GetInstance<IDbConnection>()));
            _infrastructureModule.RegisterScoped<DbContext, TDbContext>();

            // wrapping the operation: connection.open - transaction.begin - operation - flush - transaction.commit - connection.close
            _infrastructureModule.RegisterDecorator<IOperation, EfFlushOperationDecorator>();
            _infrastructureModule.RegisterDecorator<IOperation, DbTransactionOperationDecorator>();
            _infrastructureModule.RegisterDecorator<IOperation, DbConnectionOperationDecorator>();

            // making sure all changes are flushed before raising domain events so that the domain event handling reads the latest data
            _infrastructureModule.RegisterDecorator<IDomainEventAggregator, EfFlushDomainEventAggregatorDecorator>();

            _infrastructureModule.RegisterScoped(typeof(IAggregateMapping<>), _assemblies);
        }

        protected virtual DbContextOptions<TDbContext> CreateDbContextOptions(IDbConnection connection)
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            _configure.Invoke(dbContextOptionsBuilder, connection);
            return dbContextOptionsBuilder.UseLoggerFactory(_loggerFactory).Options;
        }
    }
}