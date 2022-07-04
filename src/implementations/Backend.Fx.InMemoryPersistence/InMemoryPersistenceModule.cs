using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.ConfigurationSettings;
using Backend.Fx.Features.Authorization;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.InMemoryPersistence
{
    public class InMemoryPersistenceModule : IModule
    {
        private readonly bool _withPersistentSettings;
        private readonly Type[] _aggregateRootTypes;

        public InMemoryPersistenceModule(bool withPersistentSettings, params Assembly[] assemblies)
        {
            _withPersistentSettings = withPersistentSettings;

            _aggregateRootTypes = assemblies
                .SelectMany(ass => ass
                    .GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.IsClass)
                    .Where(t => typeof(AggregateRoot).IsAssignableFrom(t)))
                .ToArray();
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            if (_withPersistentSettings)
            {
                compositionRoot.Register(
                    ServiceDescriptor.Scoped<IRepository<Setting>, InMemoryRepository<Setting>>());                
                compositionRoot.Register(
                    ServiceDescriptor.Scoped<IAggregateAuthorization<Setting>, AllowAll<Setting>>());
                compositionRoot.Register(
                    ServiceDescriptor.Singleton<IInMemoryStore<Setting>, InMemoryStore<Setting>>());
            }
            
            compositionRoot.Register(ServiceDescriptor.Scoped<ICanFlush, InMemoryFlush>());
            
            compositionRoot.Register(
                ServiceDescriptor.Singleton<IEntityIdGenerator, InMemoryEntityIdGenerator>());
            
            // loop through aggregate root types to...
            foreach (var aggregateRootType in _aggregateRootTypes)
            {
                // register the singleton store
                var genericStoreInterface = typeof(IInMemoryStore<>).MakeGenericType(aggregateRootType);
                var genericStoreImplementation = typeof(InMemoryStore<>).MakeGenericType(aggregateRootType);
                compositionRoot.Register(
                    new ServiceDescriptor(
                        genericStoreInterface,
                        genericStoreImplementation,
                        ServiceLifetime.Singleton));
                
                // ... register the Entity Framework implementation of IRepository<T>  
                var genericRepositoryInterface = typeof(IRepository<>).MakeGenericType(aggregateRootType);
                var genericRepositoryImplementation = typeof(InMemoryRepository<>).MakeGenericType(aggregateRootType);
                compositionRoot.Register(
                    new ServiceDescriptor(
                        genericRepositoryInterface,
                        genericRepositoryImplementation,
                        ServiceLifetime.Scoped));
            }
        }
    }
}