using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Domain;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Extensions.Persistence;
using Backend.Fx.Features.Authorization;
using Backend.Fx.Features.ConfigurationSettings;
using Backend.Fx.Features.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Backend.Fx.EfCore5Persistence.Bootstrapping
{
    public class EfCorePersistenceModule<TDbContext, TIdGenerator> : IModule
        where TDbContext : DbContext
        where TIdGenerator : class, IEntityIdGenerator
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly Action<DbContextOptionsBuilder<TDbContext>, IDbConnection> _configure;
        private readonly bool _withPersistentSettings;
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly Type[] _aggregateRootTypes;
        private readonly Type[] _entityTypes;
        private readonly Dictionary<Type, Type> _aggregateMappingTypes;

        public EfCorePersistenceModule(
            IDbConnectionFactory dbConnectionFactory,
            ILoggerFactory loggerFactory,
            Action<DbContextOptionsBuilder<TDbContext>, IDbConnection> configure,
            bool withPersistentSettings,
            params Assembly[] assemblies)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _loggerFactory = loggerFactory;
            _configure = configure;
            _withPersistentSettings = withPersistentSettings;

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
            compositionRoot.Register(ServiceDescriptor.Singleton<IEntityIdGenerator, TIdGenerator>());

            // at least the id generator implementation requires the IDbConnectionFactory
            compositionRoot.Register(ServiceDescriptor.Singleton<IDbConnectionFactory>(_dbConnectionFactory));
            
            // by letting the container create the connection we can be sure, that only one connection per scope is used, and disposing is done accordingly
            compositionRoot.Register(ServiceDescriptor.Scoped<IDbConnection>(_ => _dbConnectionFactory.Create()));

            // EF core requires us to flush frequently, because of a missing identity map
            compositionRoot.Register(ServiceDescriptor.Scoped<ICanFlush, EfFlush>());

            // DbContext is injected into repositories (not TDbContext!)
            compositionRoot.Register(ServiceDescriptor.Scoped<DbContext, TDbContext>());

            // TDbContext ctor requires DbContextOptions<TDbContext>, which is configured to use a container managed db connection
            compositionRoot.Register(
                ServiceDescriptor.Scoped<DbContextOptions<TDbContext>>(
                    sp =>
                    {
                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TDbContext>();
                        var dbConnection = sp.GetRequiredService<IDbConnection>();
                        _configure.Invoke(dbContextOptionsBuilder, dbConnection);
                        return dbContextOptionsBuilder.UseLoggerFactory(_loggerFactory).Options;
                    }));

            if (_withPersistentSettings)
            {
                compositionRoot.Register(
                    ServiceDescriptor.Scoped<IRepository<Setting>, EfRepository<Setting>>());                
                compositionRoot.Register(
                    ServiceDescriptor.Scoped<IAggregateMapping<Setting>, PlainAggregateMapping<Setting>>());
                compositionRoot.Register(
                    ServiceDescriptor.Scoped<IAuthorizationPolicy<Setting>, AllowAll<Setting>>());
            }
            
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
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, FlushOperationDecorator>());
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, DbContextTransactionOperationDecorator>());
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, DbConnectionOperationDecorator>());
            
            // ensure everything dirty is flushed to the db before handling domain events  
            compositionRoot.RegisterDecorator(ServiceDescriptor
                .Scoped<IDomainEventAggregator, FlushDomainEventAggregatorDecorator>());
        }
    }
}