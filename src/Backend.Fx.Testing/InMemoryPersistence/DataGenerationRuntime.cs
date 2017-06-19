namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Bootstrapping;
    using BuildingBlocks;
    using ConfigurationSettings;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using FakeItEasy;
    using JetBrains.Annotations;
    using Patterns.UnitOfWork;
    using SimpleInjector;

    public class DataGenerationRuntime : SimpleInjectorRuntime
    {
        private readonly Assembly domainAssembly;
        private readonly Action<Container> additionalContainerConfig;

        public DataGenerationRuntime(Assembly domainAssembly, [CanBeNull] Action<Container> additionalContainerConfig)
        {
            this.domainAssembly = domainAssembly;
            this.additionalContainerConfig = additionalContainerConfig;
            TenantManager = new InMemoryTenantManager(this);
            DatabaseManager = A.Fake<IDatabaseManager>();
            
            Stores = domainAssembly
                    .GetExportedTypes()
                    .Where(t => !t.GetTypeInfo().IsAbstract && t.GetTypeInfo().IsClass)
                    .Where(t => typeof(AggregateRoot).IsAssignableFrom(t))
                    .Select(t => {
                                var storeType = typeof(InMemoryStore<>).MakeGenericType(t);
                                var store = Activator.CreateInstance(storeType);
                                return new { t, store };
                            })
                    .ToDictionary(arg => arg.t, arg => arg.store);

            // the aggregate root "setting" resides outside the scanned assembly. Adding a repo manually now.
            Stores.Add(typeof(Setting), new InMemoryStore<Setting>());
        }

        public Dictionary<Type, object> Stores { get; }

        public override ITenantManager TenantManager { get; }

        public override IDatabaseManager DatabaseManager { get; }

        protected override Assembly[] Assemblies
        {
            get
            {
                return new[] {
                    typeof(InMemoryRepository<>).GetTypeInfo().Assembly,
                    domainAssembly,
                };
            }
        }

        protected override void BootPersistence()
        {
            foreach (var store in Stores)
            {
                Container.RegisterSingleton(typeof(IInMemoryStore<>).MakeGenericType(store.Key), () => store.Value);
            }

            Container.Register(typeof(IRepository<>), typeof(InMemoryRepository<>));

            var uowRegistration = Lifestyle.Scoped.CreateRegistration(() => new InMemoryUnitOfWork(new FrozenClock(), new SystemIdentity()), Container);
            Container.AddRegistration(typeof(IUnitOfWork), uowRegistration);
            Container.AddRegistration(typeof(ICanFlush), uowRegistration);
        }

        protected override void BootApplication()
        {
            Container.Register<IClock, FrozenClock>();
            additionalContainerConfig?.Invoke(Container);
        }

        protected override void InitializeJobScheduler()
        { }

        public Dictionary<Type, TAggregateRoot> GetStore<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            return (Dictionary<Type, TAggregateRoot>)Stores[typeof(TAggregateRoot)];
        }
    }
}
