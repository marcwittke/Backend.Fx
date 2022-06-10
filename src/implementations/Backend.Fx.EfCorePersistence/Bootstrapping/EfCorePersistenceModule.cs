using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.IdGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public abstract class EfCorePersistenceModule<TDbContext> : IModule
        where TDbContext : DbContext
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly Action<DbContextOptionsBuilder<TDbContext>, IDbConnection> _configure;
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IEntityIdGenerator _entityIdGenerator;
        private readonly Type[] _aggregateRootTypes;
        private readonly Type[] _entityTypes;
        private readonly Dictionary<Type, Type> _aggregateMappingTypes;

        protected EfCorePersistenceModule(
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

            _aggregateRootTypes = assemblies
                .SelectMany(ass => ass
                    .GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.IsClass)
                    .Where(t => typeof(AggregateRoot).IsAssignableFrom(t)))
                .ToArray();

            _entityTypes = assemblies
                .SelectMany(ass => ass
                    .GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.IsClass)
                    .Where(t => typeof(Entity).IsAssignableFrom(t)))
                .ToArray();

            _aggregateMappingTypes = assemblies
                .SelectMany(ass => ass
                    .GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.IsClass)
                    .Where(t => typeof(IAggregateMapping).IsAssignableFrom(t)))
                .ToDictionary(t => t.GenericTypeArguments[0], t => t);
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            // singleton id generator
            RegisterServiceDescriptor(new ServiceDescriptor(typeof(IEntityIdGenerator), _entityIdGenerator));
            
            // by letting the container create the connection we can be sure, that only one connection per scope is used, and disposing is done accordingly
            RegisterServiceDescriptor(new ServiceDescriptor(typeof(IDbConnection), sp => _dbConnectionFactory.Create(), ServiceLifetime.Scoped));

            // EF core requires us to flush frequently, because of a missing identity map
            RegisterServiceDescriptor(new ServiceDescriptor(typeof(ICanFlush), typeof(EfFlush)));

            // DbContext is injected into repositories (not TDbContext!)
            RegisterServiceDescriptor(new ServiceDescriptor(typeof(DbContext), typeof(TDbContext)));

            // TDbContext ctor requires DbContextOptions<TDbContext>
            RegisterServiceDescriptor(new ServiceDescriptor(typeof(DbContextOptions<TDbContext>), sp => 
            {
                IDbConnection connection = sp.GetRequiredService<IDbConnection>();
                var dbContextOptionsBuilder = new DbContextOptionsBuilder<TDbContext>();
                _configure.Invoke(dbContextOptionsBuilder, connection);
                return dbContextOptionsBuilder.UseLoggerFactory(_loggerFactory).Options;
            }, ServiceLifetime.Scoped));

            foreach (var aggregateRootType in _aggregateRootTypes)
            {
                // EF Repository for this aggregate root
                RegisterServiceDescriptor(new ServiceDescriptor(
                    typeof(IRepository<>).MakeGenericType(aggregateRootType),
                    typeof(EfRepository<>).MakeGenericType(aggregateRootType),
                    ServiceLifetime.Scoped));

                // aggregate mapping definition
                RegisterServiceDescriptor(new ServiceDescriptor(
                    typeof(IAggregateMapping<>).MakeGenericType(aggregateRootType),
                    sp => sp.GetService(_aggregateMappingTypes[aggregateRootType]),
                    ServiceLifetime.Scoped));
            }

            foreach (var entityType in _entityTypes)
            {
                // IQueryable is supported, but should be use with caution, since it bypasses authorization
                RegisterServiceDescriptor(new ServiceDescriptor(
                    typeof(IQueryable<>).MakeGenericType(entityType),
                    typeof(EntityQueryable<>).MakeGenericType(entityType),
                    ServiceLifetime.Scoped));
            }

            // wrapping the operation: connection.open - transaction.begin - operation - (flush) - transaction.commit - connection.close
            compositionRoot.InfrastructureModule.RegisterDecorator<IOperation, FlushOperationDecorator>();
            compositionRoot.InfrastructureModule.RegisterDecorator<IOperation, DbContextTransactionOperationDecorator>();
            compositionRoot.InfrastructureModule.RegisterDecorator<IOperation, DbConnectionOperationDecorator>();

            // ensure everything dirty is flushed to the db before handling domain events  
            compositionRoot.InfrastructureModule.RegisterDecorator<IDomainEventAggregator, FlushDomainEventAggregatorDecorator>();
        }
    }
}