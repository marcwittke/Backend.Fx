using Backend.Fx.SimpleInjectorDependencyInjection;

namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;
    using Patterns.IdGeneration;
    using RandomData;

    public abstract class InMemoryPersistenceFixture
    {
        private readonly Dictionary<Type, object> stores;

        protected InMemoryPersistenceFixture(bool withDemoData, Assembly domainAssembly, params IModule[] modules)
            : this(withDemoData, new[] { domainAssembly }, modules)
        {}

        protected InMemoryPersistenceFixture(bool withDemoData, Assembly[] domainAssemblies, params IModule[] modules)
        {
            using (SimpleInjectorCompositionRoot compositionRoot = new SimpleInjectorCompositionRoot())
            {
                var inMemoryPersistenceModule = new InMemoryPersistenceModule(domainAssemblies);
                var inMemoryIdGeneratorsModule = new InMemoryIdGeneratorsModule();
                var inMemoryApplicationModule = new InMemoryDomainModule(domainAssemblies);
                compositionRoot.RegisterModules(inMemoryApplicationModule, inMemoryIdGeneratorsModule, inMemoryPersistenceModule);
                compositionRoot.RegisterModules(modules);
                compositionRoot.Verify();

                ITenantInitializer tenantInitializer = new TenantInitializer(compositionRoot);
                ITenantManager tenantManager = new InMemoryTenantManager(tenantInitializer);

                // create and fill a tenant
                var tenantId = withDemoData
                               ? tenantManager.CreateDemonstrationTenant("test", "", false, new CultureInfo("en-US"))
                               : tenantManager.CreateProductionTenant("test", "", false, new CultureInfo("en-US"));
                tenantManager.EnsureTenantIsInitialized(tenantId);
                TenantIdHolder.ReplaceCurrent(tenantId);

                // from now on we do not use the composition root any more, but we save the filled in memory repositories
                stores = inMemoryPersistenceModule.Stores;
                // and the in memory entity id generator
                EntityIdGenerator = inMemoryIdGeneratorsModule.EntityIdGenerator;
            }
            
        }

        public CurrentTenantIdHolder TenantIdHolder { get; } = new CurrentTenantIdHolder();

        public IEntityIdGenerator EntityIdGenerator { get; }

        public TAggregateRoot GetRandom<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            return GetRepository<TAggregateRoot>().AggregateQueryable.Random();
        }

        public InMemoryQueryable<TAggregateRoot> GetQueryable<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            IInMemoryStore<TAggregateRoot> store = (IInMemoryStore<TAggregateRoot>) stores[typeof(TAggregateRoot)];
            return new InMemoryQueryable<TAggregateRoot>(store);
        } 

        public InMemoryRepository<TAggregateRoot> GetRepository<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            return new InMemoryRepository<TAggregateRoot>((IInMemoryStore<TAggregateRoot>)stores[typeof(TAggregateRoot)], TenantIdHolder, new AllowAll<TAggregateRoot>());
        }

        public void ClearRepository<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            GetRepository<TAggregateRoot>().Clear();
        }
    }

    public abstract class InMemoryPersistenceWithProdDataFixture : InMemoryPersistenceFixture
    {
        protected InMemoryPersistenceWithProdDataFixture(Assembly[] domainAssemblies, params IModule[] modules)
            : base(false, domainAssemblies, modules)
        { }
    }

    public abstract class InMemoryPersistenceWithDemoDataFixture : InMemoryPersistenceFixture
    {
        protected InMemoryPersistenceWithDemoDataFixture(Assembly[] domainAssemblies,params IModule[] modules)
            : base(true, domainAssemblies, modules)
        { }
    }
}