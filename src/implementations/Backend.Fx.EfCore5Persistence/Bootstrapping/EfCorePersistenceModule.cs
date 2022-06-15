using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.IdGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.EfCore5Persistence.Bootstrapping
{
    public class EfCorePersistenceModule<TDbContext> : IModule
        where TDbContext : DbContext
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly Action<DbContextOptionsBuilder<TDbContext>, IDbConnection> _configure;
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IEntityIdGenerator _entityIdGenerator;
        private readonly Type[] _aggregateRootTypes;
        private readonly Type[] _entityTypes;
        private readonly Dictionary<Type, Type> _aggregateMappingTypes;

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
                .ToDictionary(
                    t => t
                        .GetInterfaces()
                        .Single(i => i.GenericTypeArguments.Length == 1
                                     && typeof(AggregateRoot).IsAssignableFrom(i.GenericTypeArguments[0]))
                        .GenericTypeArguments[0],
                    t => t);
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            // singleton id generator
            compositionRoot.Register(
                new ServiceDescriptor(
                    typeof(IEntityIdGenerator),
                    _entityIdGenerator));

            // by letting the container create the connection we can be sure, that only one connection per scope is used, and disposing is done accordingly
            compositionRoot.Register(
                new ServiceDescriptor(
                    typeof(IDbConnection),
                    sp => _dbConnectionFactory.Create(),
                    ServiceLifetime.Scoped));

            // EF core requires us to flush frequently, because of a missing identity map
            compositionRoot.Register(
                new ServiceDescriptor(
                    typeof(ICanFlush),
                    typeof(EfFlush),
                    ServiceLifetime.Scoped));

            // DbContext is injected into repositories (not TDbContext!)
            compositionRoot.Register(
                new ServiceDescriptor(
                    typeof(DbContext),
                    typeof(TDbContext),
                    ServiceLifetime.Scoped));

            // TDbContext ctor requires DbContextOptions<TDbContext>, which is configured to use a container managed db connection
            compositionRoot.Register(
                new ServiceDescriptor(typeof(DbContextOptions<TDbContext>),
                    sp =>
                    {
                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TDbContext>();
                        var dbConnection = sp.GetRequiredService<IDbConnection>();
                        _configure.Invoke(dbContextOptionsBuilder, dbConnection);

                        return dbContextOptionsBuilder.UseLoggerFactory(_loggerFactory).Options;
                    }, ServiceLifetime.Scoped));

            // loop through aggregate root types to...
            foreach (var aggregateRootType in _aggregateRootTypes)
            {
                // ... register the Entity Framework implementation of IRepository<T>  
                var genericRepositoryInterface = typeof(IRepository<>).MakeGenericType(aggregateRootType);
                var genericRepositoryImplementation = typeof(EfRepository<>).MakeGenericType(aggregateRootType);
                compositionRoot.Register(
                    new ServiceDescriptor(
                        genericRepositoryInterface,
                        genericRepositoryImplementation,
                        ServiceLifetime.Scoped));

                // ... register the aggregate mapping definition (singleton)
                var genericAggregateMappingInterface = typeof(IAggregateMapping<>).MakeGenericType(aggregateRootType);
                var aggregateMappingImplementation = _aggregateMappingTypes[aggregateRootType];
                compositionRoot.Register(
                    new ServiceDescriptor(
                        genericAggregateMappingInterface,
                        aggregateMappingImplementation,
                        ServiceLifetime.Singleton));
            }

            // loop through entity types ...
            foreach (var entityType in _entityTypes)
            {
                // to register the Entity Framework implementation of IQueryable<T>
                compositionRoot.Register(new ServiceDescriptor(
                    typeof(IQueryable<>).MakeGenericType(entityType),
                    typeof(EntityQueryable<>).MakeGenericType(entityType),
                    ServiceLifetime.Scoped));
            }

            // wrapping the operation:
            //   invoke   -> connection.open  -> transaction.begin ---+
            //                                                        |
            //                                                        v
            //                                                      operation
            //                                                        |
            //                                                        v
            //                                                      flush
            //                                                        |
            // end invoke <- connection.close <- transaction.commit <-+ 
            compositionRoot.RegisterDecorator(
                new ServiceDescriptor(
                    typeof(IOperation),
                    typeof(FlushOperationDecorator),
                    ServiceLifetime.Scoped));
            compositionRoot.RegisterDecorator(
                new ServiceDescriptor(
                    typeof(IOperation),
                    typeof(DbContextTransactionOperationDecorator),
                    ServiceLifetime.Scoped));
            compositionRoot.RegisterDecorator(
                new ServiceDescriptor(
                    typeof(IOperation),
                    typeof(DbConnectionOperationDecorator),
                    ServiceLifetime.Scoped));

            // // ensure everything dirty is flushed to the db before handling domain events  
            // compositionRoot.Register(
            //     new ServiceDescriptor(
            //         typeof(IDomainEventAggregator),
            //         typeof(FlushDomainEventAggregatorDecorator),
            //         ServiceLifetime.Scoped));
        }
    }
}